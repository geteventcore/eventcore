using EventCore.EventSourcing;
using EventCore.Samples.SimpleEventStore.StreamDb;
using EventCore.Samples.SimpleEventStore.StreamDb.DbModels;
using EventCore.Utilities;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.SimpleEventStore.Client
{
	public class StreamClient : IStreamClient
	{
		private readonly IStandardLogger _logger;
		private readonly StreamDbContext _db;
		private readonly string _notificationsHubUrl;

		// Valid stream characters:
		// - ASCII letters and numbers
		// - underscore ("_")
		// - short dash / minus sign ("-")
		// - dollar sign
		public const string INVALID_STREAM_ID_REGEX = @"[^A-Za-z0-9_\-$]";

		// Special naming for stream ids starting with "$"... these will be
		// treated as subscription names and queries as such instead of querying on stream id.
		// Special case stream id == "$" with no additional characters, this will be
		// treated as a built-in subscription capturing all stream ids.
		public const string SUBSCRIPTION_STREAM_ID_PREFIX = "$";

		// Note that first stream position will be used for global index positions, so we expect
		// StreamEventDbModel.GlobalIndex to start at this value.
		public long FirstPositionInStream => 1;

		public StreamClient(IStandardLogger logger, StreamDbContext db, string notificationsHubUrl)
		{
			_logger = logger;
			_db = db;
			_notificationsHubUrl = notificationsHubUrl;
		}

		public static bool ValidateStreamIdChars(string chars) => !Regex.IsMatch(chars, INVALID_STREAM_ID_REGEX);
		public static bool IsSubscriptionStreamId(string streamId) => streamId.StartsWith(SUBSCRIPTION_STREAM_ID_PREFIX);

		public async Task<CommitResult> CommitEventsToStreamAsync(string streamId, long? expectedLastPosition, IEnumerable<CommitEvent> events)
		{
			if (!ValidateStreamIdChars(streamId))
			{
				throw new ArgumentException("Invalid character(s) in stream id.");
			}

			if (streamId.StartsWith(SUBSCRIPTION_STREAM_ID_PREFIX))
			{
				throw new ArgumentException("Can't commit to a subscription stream.");
			}

			var lastPosition = _db.GetMaxStreamPositionByStreamId(streamId);

			if (lastPosition == null && expectedLastPosition.HasValue)
			{
				throw new InvalidOperationException("Expected last position is past the end of stream.");
			}

			if (lastPosition != expectedLastPosition)
			{
				return CommitResult.ConcurrencyConflict;
			}

			if (events.Count() == 0) return CommitResult.Success;

			var eventNo = lastPosition.GetValueOrDefault(FirstPositionInStream - 1) + 1;
			var newStreamEvents = new List<StreamEventDbModel>();
			foreach (var commitEvent in events)
			{
				var newStreamEvent = new StreamEventDbModel() { StreamId = streamId, EventType = commitEvent.EventType, StreamPosition = eventNo, EventData = commitEvent.Data };
				_db.StreamEvent.Add(newStreamEvent);
				newStreamEvents.Add(newStreamEvent);
				eventNo++;
			}

			await _db.SaveChangesAsync();

			var latestGlobalId = newStreamEvents.Max(x => x.GlobalPosition);

			// Publish notification but don't wait for it to complete because
			// it's not critical. It's more important to return to the caller as
			// quickly as possible.
			var _ = PublishCommitNotification(latestGlobalId);

			return CommitResult.Success;
		}

		public Task<long?> GetLastPositionInStreamAsync(string streamId)
		{
			if (IsSubscriptionStreamId(streamId))
			{
				if (streamId.Length == 1) return Task.FromResult(_db.GetMaxGlobalPosition());
				else throw new ArgumentException("Invalid stream id.");
			}
			else
			{
				return Task.FromResult(_db.GetMaxStreamPositionByStreamId(streamId));
			}
		}

		private Func<IOrderedQueryable<StreamEventDbModel>> BuildEventQuery(bool isSubscription, string streamId, long fromPosition)
		{
			if (isSubscription)
			{
				// If streamId == "$" - i.e. built-in subscription to listen on all streams.
				if (streamId.Length == 1)
				{
					return () => _db.StreamEvent.Where(x => x.GlobalPosition >= fromPosition).OrderBy(x => x.StreamPosition);
				}
				else
				{
					return () =>
						_db.SubscriptionFilter
							.Where(x => x.SubscriptionName == streamId)
							.SelectMany(sub => _db.StreamEvent.Where(se => se.StreamId.ToUpper().StartsWith(sub.StreamIdPrefix.ToUpper())))
							.Distinct()
							.OrderBy(x => x.GlobalPosition);
				}
			}
			else
			{
				return () =>
					_db.StreamEvent
						.Where(x => x.StreamId == streamId && x.StreamPosition >= fromPosition) // No string transformation (i.e. ToUpper) assuming db is case insensitive by default.
						.OrderBy(x => x.StreamPosition);
			}
		}

		public async Task LoadStreamEventsAsync(string streamId, long fromPosition, Func<StreamEvent, Task> receiverAsync, CancellationToken cancellationToken)
		{
			var isSubscription = IsSubscriptionStreamId(streamId);
			var eventQuery = BuildEventQuery(isSubscription, streamId, fromPosition);

			foreach (var streamEvent in eventQuery())
			{
				if (!cancellationToken.IsCancellationRequested)
				{
					var primaryStreamId = streamEvent.StreamId;
					var primaryPosition = streamEvent.StreamPosition;
					StreamEventLink link = null;

					if (isSubscription)
					{
						link = new StreamEventLink(streamEvent.StreamId, streamEvent.StreamPosition);
						primaryStreamId = streamId;
						primaryPosition = streamEvent.GlobalPosition;
					}

					// _logger.LogInformation($"Loaded event {streamEvent.EventType} ({primaryPosition}) from stream {streamId}.");

					// Don't catch - if the receiver throws an exception let it bubble up to the caller.
					await receiverAsync(new StreamEvent(primaryStreamId, primaryPosition, link, streamEvent.EventType, streamEvent.EventData));
				}
			}
		}

		public async Task SubscribeToStreamAsync(string streamId, long fromPosition, Func<StreamEvent, Task> receiverAsync, CancellationToken cancellationToken)
		{
			long? lastLoadedGlobalPosition = null;
			var advancementTrigger = new ManualResetEventSlim(false); // Set when global position is advanced.

			var isSubscription = IsSubscriptionStreamId(streamId);

			var connection = new HubConnectionBuilder().WithUrl(_notificationsHubUrl).Build();

			connection.Closed += async (ex) =>
			{
				var delayMs = 1000;
				_logger.LogError(ex, $"Notifications hub connection closed, trying again in {delayMs} milliseconds.");
				await Task.Delay(delayMs);
				await connection.StartAsync();
			};

			connection.On<long>("ReceiveClientNotification", (newGlobalPosition) =>
			{
				if (newGlobalPosition > lastLoadedGlobalPosition.GetValueOrDefault(fromPosition - 1))
				{
					advancementTrigger.Set();
				}
			});

			await connection.StartAsync();

			var eventQuery = BuildEventQuery(isSubscription, streamId, lastLoadedGlobalPosition.GetValueOrDefault(fromPosition - 1) + 1);

			while (!cancellationToken.IsCancellationRequested)
			{
				foreach (var streamEvent in eventQuery())
				{
					var primaryStreamId = streamEvent.StreamId;
					var primaryPosition = streamEvent.StreamPosition;
					StreamEventLink link = null;

					if (isSubscription)
					{
						link = new StreamEventLink(streamEvent.StreamId, streamEvent.StreamPosition);
						primaryStreamId = streamId;
						primaryPosition = streamEvent.GlobalPosition;
					}

					// Don't catch - if the receiver throws an exception let it bubble up to the caller.
					await receiverAsync(new StreamEvent(primaryStreamId, primaryPosition, link, streamEvent.EventType, streamEvent.EventData));

					// Advance position tracking so we don't send duplicate events.
					lastLoadedGlobalPosition = streamEvent.GlobalPosition;
				}

				await Task.WhenAny(new[] { cancellationToken.WaitHandle.AsTask(), advancementTrigger.WaitHandle.AsTask() });
				if (cancellationToken.IsCancellationRequested) break;
				advancementTrigger.Reset();
			}
		}

		private async Task PublishCommitNotification(long globalPosition)
		{
			var connection = new HubConnectionBuilder().WithUrl(_notificationsHubUrl).Build();
			var isDone = false;

			connection.Closed += async (ex) =>
			{
				if (!isDone)
				{
					var delayMs = new Random().Next(500, 1500);
					_logger.LogError(ex, $"Notifications hub connection closed, trying again in {delayMs} milliseconds.");
					await Task.Delay(delayMs);
					await connection.StartAsync();
				}
			};

			try
			{
				await connection.StartAsync();
				await connection.InvokeAsync("ReceiveCommitNotification", globalPosition);
			}
			catch (Exception ex1)
			{
				var delayMs = 3000;
				_logger.LogError(ex1, $"Exception while publishing commit notification. Trying one more time after {delayMs} milliseconds.");
				await Task.Delay(delayMs);

				try
				{
					await connection.StartAsync();
					await connection.InvokeAsync("ReceiveCommitNotification", globalPosition);
				}
				catch (Exception ex2)
				{
					_logger.LogError(ex2, $"Exception while publishing (retry) commit notification. Giving up.");
				}
			}

			if (connection.State == HubConnectionState.Connected)
			{
				isDone = true;
				await connection.StopAsync();
			}
		}

		// Stream client uses db context, expected to be disposed by caller (via dependency injection, etc.)
		public void Dispose() { }
	}
}
