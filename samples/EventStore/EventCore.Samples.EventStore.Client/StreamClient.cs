using EventCore.EventSourcing;
using EventCore.Samples.EventStore.StreamDb;
using EventCore.Samples.EventStore.StreamDb.DbModels;
using EventCore.Utilities;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.EventStore.Client
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
		public const string INVALID_STREAM_ID_REGEX = @"[^A-Za-z0-9_-$]";

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

			var lastPosition = _db.GetLastStreamPositionByStreamId(streamId);

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
			return Task.FromResult(_db.GetLastStreamPositionByStreamId(streamId));
		}

		public async Task LoadStreamEventsAsync(string streamId, long fromPosition, Func<StreamEvent, Task> receiverAsync, CancellationToken cancellationToken)
		{
			var isSubscription = IsSubscriptionStreamId(streamId);

			Func<string, long, IEnumerable<StreamEventDbModel>> eventQuery;

			if (isSubscription)
			{
				if (streamId.Length == 1) eventQuery = (_1, _2) => _db.GetAllStreamEventsByGlobalPosition(fromPosition);
				else eventQuery = (_1, _2) => _db.GetSubscriptionEventsByGlobalPosition(streamId, fromPosition);
			}
			else
			{
				eventQuery = (_1, _2) => _db.GetAllStreamEventsByGlobalPosition(fromPosition);
			}

			foreach (var streamEvent in eventQuery(streamId, fromPosition))
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

			Func<string, long, IEnumerable<StreamEventDbModel>> eventQuery;

			if (isSubscription)
			{
				if (streamId.Length == 1) eventQuery = (_1, _2) => _db.GetAllStreamEventsByGlobalPosition(fromPosition);
				else eventQuery = (_1, _2) => _db.GetSubscriptionEventsByGlobalPosition(streamId, fromPosition);
			}
			else
			{
				eventQuery = (_1, _2) => _db.GetAllStreamEventsByGlobalPosition(fromPosition);
			}

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

			while (!cancellationToken.IsCancellationRequested)
			{
				foreach (var streamEvent in eventQuery(streamId, lastLoadedGlobalPosition.GetValueOrDefault(fromPosition - 1) + 1))
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

		private async Task PublishCommitNotification(long globalIndex)
		{
			var connection = new HubConnectionBuilder().WithUrl(_notificationsHubUrl).Build();

			connection.Closed += async (ex) =>
			{
				await Task.Delay(1000); // Wait 1 second, then try again.
				_logger.LogError(ex, "Notifications hub connection closed, trying again.");
				await connection.StartAsync();
			};

			try
			{
				await connection.StartAsync();
				await connection.InvokeAsync("ReceiveServerNotification", globalIndex);
			}
			catch (Exception ex1)
			{
				var delayMs = 3000;
				_logger.LogError(ex1, $"Exception while publishing commit notification. Trying one more time after {delayMs} milliseconds.");
				await Task.Delay(delayMs);

				try
				{
					await connection.StartAsync();
					await connection.InvokeAsync("ReceiveCommitNotification", globalIndex);
				}
				catch (Exception ex2)
				{
					_logger.LogError(ex2, $"Exception while publishing (retry) commit notification. Giving up.");
				}
			}

			if (connection.State == HubConnectionState.Connected)
			{
				await connection.StopAsync();
			}
		}

		// Stream client uses db context, expected to be disposed by caller (via dependency injection, etc.)
		public void Dispose() { }
	}
}
