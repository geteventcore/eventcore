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

namespace EventCore.Samples.EventStore.StreamClient
{
	public class EventStoreStreamClient : IStreamClient
	{
		// NOTE: Stream ids are case SENSITIVE in GY's Event Store.
		// Haven't figured out how to disable this yet, or if possible.

		private const string INVALID_STREAM_ID_REGEX = @"[^A-Za-z0-9_\-$]";

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

		public async Task<CommitResult> CommitEventsToStreamAsync(string streamId, long? expectedLastPosition, IEnumerable<CommitEvent> events)
		{
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
				Func<EventStoreCatchUpSubscription, ResolvedEvent, Task> eventAppeared = async (_, resolvedEvent) =>
				{
					await ReceiveResolvedEventAsync(receiverAsync, resolvedEvent, cancellationToken);
				};

				using (var sub = new CatchUpSubscription(_connection, streamId, fromPosition, eventAppeared))
				{
					await cancellationToken.WaitHandle.AsTask();
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

		// Wrapper class for auto renewing dropped subscription.
		private class CatchUpSubscription : IDisposable
		{
			private readonly IEventStoreConnection _connection;
			private readonly string _streamId;
			private readonly Func<EventStoreCatchUpSubscription, ResolvedEvent, Task> _eventAppeared;

			private readonly CatchUpSubscriptionSettings _settings = new CatchUpSubscriptionSettings(
					CatchUpSubscriptionSettings.Default.MaxLiveQueueSize,
					CatchUpSubscriptionSettings.Default.ReadBatchSize,
					false, true);

			private EventStoreStreamCatchUpSubscription _sub;
			private long? _lastEventPosition;

			public CatchUpSubscription(IEventStoreConnection connection, string streamId, long fromPosition, Func<EventStoreCatchUpSubscription, ResolvedEvent, Task> eventAppeared)
			{
				_connection = connection;
				_streamId = streamId;
				_lastEventPosition = fromPosition == Constants.FIRST_POSITION_IN_STREAM ? (long?)null : fromPosition - 1;

				_eventAppeared = (sub, resolvedEvent) =>
				{
					_lastEventPosition = resolvedEvent.OriginalEventNumber;
					return eventAppeared(sub, resolvedEvent);
				};

				Subscribe();
			}

			private void Subscribe()
			{
				Console.WriteLine("Starting subscribe from: " + _lastEventPosition);
				_sub = _connection.SubscribeToStreamFrom(_streamId, _lastEventPosition, _settings, _eventAppeared, null, SubscriptionDropped, null);
			}

			private void SubscriptionDropped(EventStoreCatchUpSubscription subscription, SubscriptionDropReason reason, Exception ex)
			{
				// Known bug in ES, must manually stop subscription or sub will continue delivering events.
				// https://groups.google.com/forum/#!searchin/event-store/subscription/event-store/AdKzv8TxabM/6RzudeuAAgAJ
				_sub.Stop(); 
				Subscribe();
			}

			private bool disposedValue = false; // To detect redundant calls

			protected virtual void Dispose(bool disposing)
			{
				if (!disposedValue)
				{
					if (disposing)
					{
						_sub.Stop();
					}

					disposedValue = true;
				}
			}

			public void Dispose() => Dispose(true);
		}

		public void Dispose()
		{
			// nothing...
		}
	}
}
