using EventCore.EventSourcing;
using EventCore.Utilities;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.GYEventStore.StreamClient
{
	public class EventStoreStreamClient : IStreamClient
	{
		// NOTE: Stream ids are case SENSITIVE in GY's Event Store.
		// Haven't figured out how to disable this yet.
		// All stream ids will be upper cased.

		private const string INVALID_STREAM_ID_REGEX = @"[^A-Z0-9_\-$]";

		private readonly IStandardLogger _logger;
		private readonly IEventStoreConnection _connection;
		private readonly EventStoreStreamClientOptions _options;

		public long FirstPositionInStream => Constants.FIRST_POSITION_IN_STREAM;

		public EventStoreStreamClient(IStandardLogger logger, IEventStoreConnection connection, EventStoreStreamClientOptions options)
		{
			_logger = logger;
			_connection = connection;
			_options = options;
		}

		private static bool ValidateStreamIdChars(string chars) => !Regex.IsMatch(chars, INVALID_STREAM_ID_REGEX);

		public IEventStoreConnection CreateConnection(string uri)
		{

			var cnn = EventStoreConnection.Create(uri);
			cnn.Closed += new EventHandler<ClientClosedEventArgs>(delegate (Object o, ClientClosedEventArgs a)
			{
				_logger.LogWarning($"Event Store connection closed. Reconnecting after {_options.ReconnectDelaySeconds} seconds.");
				Thread.Sleep(_options.ReconnectDelaySeconds * 1000);
				a.Connection.ConnectAsync().Wait();
			});
			return cnn;
		}

		public async Task<CommitResult> CommitEventsToStreamAsync(string streamId, long? expectedLastPosition, IEnumerable<CommitEvent> events)
		{
			streamId = streamId.ToUpper(); // GY Event Store uses case sensitive stream ids. Neutralize here.

			if (!ValidateStreamIdChars(streamId))
			{
				throw new ArgumentException("Invalid character(s) in stream id.");
			}

			if (expectedLastPosition.HasValue && expectedLastPosition < FirstPositionInStream)
			{
				throw new ArgumentException("Invalid expected last position.");
			}

			try
			{

				long expectedVersion = expectedLastPosition.GetValueOrDefault(ExpectedVersion.NoStream);

				if (events.Count() == 0) return CommitResult.Success;

				await _connection.AppendToStreamAsync(streamId, expectedVersion, MapCommitEvents(events));

				return CommitResult.Success;
			}
			catch (WrongExpectedVersionException)
			{
				return CommitResult.ConcurrencyConflict;
			}
		}

		private IEnumerable<EventData> MapCommitEvents(IEnumerable<CommitEvent> events)
		{
			foreach (var e in events)
			{
				yield return new EventData(Guid.NewGuid(), e.EventType, true, e.Data, null);
			}
		}

		public async Task LoadStreamEventsAsync(string streamId, long fromPosition, Func<StreamEvent, Task> receiverAsync, CancellationToken cancellationToken)
		{
			if (fromPosition < FirstPositionInStream)
			{
				throw new ArgumentException("Invalid position.");
			}

			try
			{
				streamId = streamId.ToUpper(); // GY Event Store uses case sensitive stream ids. Neutralize here.

				StreamEventsSlice currentSlice;
				long nextSliceStart = StreamPosition.Start;

				do
				{
					// Read stream with to-links resolution.
					currentSlice = await _connection.ReadStreamEventsForwardAsync(streamId, nextSliceStart, _options.StreamReadBatchSize, true);

					if (currentSlice.Status != SliceReadStatus.Success)
					{
						break;
					}

					nextSliceStart = currentSlice.NextEventNumber;

					foreach (var resolvedEvent in currentSlice.Events)
					{
						await ReceiveResolvedEventAsync(receiverAsync, resolvedEvent, cancellationToken);
					}

				} while (!currentSlice.IsEndOfStream && !cancellationToken.IsCancellationRequested);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception while loading stream.");
				throw;
			}
		}

		public async Task SubscribeToStreamAsync(string streamId, long fromPosition, Func<StreamEvent, Task> receiverAsync, CancellationToken cancellationToken)
		{
			if (fromPosition < FirstPositionInStream)
			{
				throw new ArgumentException("Invalid position.");
			}

			try
			{
				streamId = streamId.ToUpper(); // GY Event Store uses case sensitive stream ids. Neutralize here.

				var subSettings = new CatchUpSubscriptionSettings(
					CatchUpSubscriptionSettings.Default.MaxLiveQueueSize,
					CatchUpSubscriptionSettings.Default.ReadBatchSize,
					false, true);

				var sub = _connection.SubscribeToStreamFrom(
				 streamId,
				 // Client api expects last checkpoint or null if none.
				 fromPosition == FirstPositionInStream ? (long?)null : fromPosition - 1,
				 subSettings,
				 async (_, resolvedEvent) =>
				 {
					 await ReceiveResolvedEventAsync(receiverAsync, resolvedEvent, cancellationToken);
				 },
				 null,
				 (_, reason, ex) =>
				 {
					 _logger.LogError(ex, $"Subscription dropped. ({reason})");
				 },
				 null);

				await cancellationToken.WaitHandle.AsTask();

				try
				{
					sub.Stop(); // Does not block.
				}
				catch (Exception)
				{
					// Ignore errors.
				}

			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception while subscribing to stream.");
				throw;
			}
		}

		private async Task ReceiveResolvedEventAsync(Func<StreamEvent, Task> receiverAsync, ResolvedEvent resolvedEvent, CancellationToken cancellationToken)
		{
			if (!cancellationToken.IsCancellationRequested)
			{
				var streamId = resolvedEvent.Event.EventStreamId;
				var position = resolvedEvent.Event.EventNumber;
				StreamEventLink link = null;

				if (resolvedEvent.Link != null)
				{
					link = new StreamEventLink(resolvedEvent.Event.EventStreamId, resolvedEvent.Event.EventNumber);
					streamId = resolvedEvent.Link.EventStreamId;
					position = resolvedEvent.Link.EventNumber;
				}

				var streamEvent = new StreamEvent(resolvedEvent.OriginalStreamId, resolvedEvent.OriginalEventNumber, link, resolvedEvent.Event.EventType, resolvedEvent.Event.Data);

				// Send the assembled stream event to the receiver.
				await receiverAsync(streamEvent);
			}
		}

		public async Task<long?> GetLastPositionInStreamAsync(string streamId)
		{
			try
			{
				long? lastPosition = null;

				var result = await _connection.ReadEventAsync(streamId, StreamPosition.End, false);

				if (result.Status == EventReadStatus.Success)
				{
					if (result.Event.HasValue)
					{
						lastPosition = result.Event.Value.OriginalEventNumber;
					}
				}

				return lastPosition;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception while getting end of stream.");
				throw;
			}
		}

		public void Dispose()
		{
			// nothing...
		}
	}
}
