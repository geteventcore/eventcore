using EventCore.Utilities;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.EventSourcing.EventStore
{
	public class EventStoreStreamClient : IStreamClient
	{
		private const string INVALID_STREAM_ID_REGEX = @"[^A-Za-z0-9._-]";

		private readonly IGenericLogger _logger;
		private readonly IEventStoreConnectionFactory _connectionFactory;
		private readonly EventStoreStreamClientOptions _options;

		public long FirstPositionInStream => Constants.FIRST_POSITION_IN_STREAM;

		public EventStoreStreamClient(IGenericLogger logger, IEventStoreConnectionFactory connectionFactory, EventStoreStreamClientOptions options)
		{
			_logger = logger;
			_connectionFactory = connectionFactory;
			_options = options;
		}

		public async Task<CommitResult> CommitEventsToStreamAsync(string region, string streamId, long? expectedLastPosition, IEnumerable<CommitEvent> events)
		{
			if (Regex.IsMatch(streamId, INVALID_STREAM_ID_REGEX))
			{
				throw new ArgumentException("Invalid character(s) in stream id.");
			}

			if (expectedLastPosition.HasValue && expectedLastPosition < FirstPositionInStream)
			{
				throw new ArgumentException("Invalid character(s) in stream id.");
			}

			try
			{

				long expectedVersion = expectedLastPosition.GetValueOrDefault(ExpectedVersion.NoStream);

				if (events.Count() == 0) return CommitResult.Success;

				using (var conn = _connectionFactory.Create(region))
				{
					await conn.ConnectAsync();
					await conn.AppendToStreamAsync(streamId, expectedVersion, MapCommitEvents(events));
					conn.Close();
				}

				return CommitResult.Success;
			}
			catch (WrongExpectedVersionException)
			{
				return CommitResult.ConcurrencyConflict;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception while committing events.");
				return CommitResult.Error;
			}
		}

		private IEnumerable<EventData> MapCommitEvents(IEnumerable<CommitEvent> events)
		{
			foreach (var e in events)
			{
				yield return new EventData(Guid.NewGuid(), e.EventType, true, e.Payload, null);
			}
		}

		public async Task LoadStreamEventsAsync(string region, string streamId, long fromPosition, Func<StreamEvent, CancellationToken, Task> receiverAsync, CancellationToken cancellationToken)
		{
			if (fromPosition < FirstPositionInStream)
			{
				throw new ArgumentException("Invalid position.");
			}

			try
			{
				StreamEventsSlice currentSlice;
				long nextSliceStart = StreamPosition.Start;

				using (var conn = _connectionFactory.Create(region))
				{
					await conn.ConnectAsync();

					do
					{
						// Read stream with to-links resolution.
						currentSlice = await conn.ReadStreamEventsForwardAsync(streamId, nextSliceStart, _options.StreamReadBatchSize, true);

						if (currentSlice.Status != SliceReadStatus.Success)
						{
							break;
						}

						nextSliceStart = currentSlice.NextEventNumber;

						foreach (var resolvedEvent in currentSlice.Events)
						{
							if (!cancellationToken.IsCancellationRequested)
							{
								var e = resolvedEvent.Event; // The base event that this event links to or is.
								var streamEvent = new StreamEvent(e.EventStreamId, e.EventNumber, e.EventType, e.Data);

								// Send the assembled stream event to the receiver.
								await receiverAsync(streamEvent, cancellationToken);
							}
						}

					} while (!currentSlice.IsEndOfStream && !cancellationToken.IsCancellationRequested);

					conn.Close();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception while loading stream.");
				throw;
			}
		}

		public async Task SubscribeToStreamAsync(string region, string streamId, long fromPosition, Func<StreamEvent, CancellationToken, Task> receiverAsync, CancellationToken cancellationToken)
		{
			if (fromPosition < FirstPositionInStream)
			{
				throw new ArgumentException("Invalid position.");
			}

			try
			{
				var subSettings = new CatchUpSubscriptionSettings(
					CatchUpSubscriptionSettings.Default.MaxLiveQueueSize,
					CatchUpSubscriptionSettings.Default.ReadBatchSize,
					false, true);

				using (var conn = _connectionFactory.Create(region))
				{
					await conn.ConnectAsync();

					var sub = conn.SubscribeToStreamFrom(
					 streamId, fromPosition, subSettings,
					 async (_, resolvedEvent) =>
					 {
						 Console.WriteLine("Received...");
						 if (!cancellationToken.IsCancellationRequested)
						 {
							 var e = resolvedEvent.Event; // The base event that this event links to or is.
							 var streamEvent = new StreamEvent(e.EventStreamId, e.EventNumber, e.EventType, e.Data);

							 // Send the assembled stream event to the receiver.
							 await receiverAsync(streamEvent, cancellationToken);
						 }
					 },
					 null,
					 (_, reason, ex) =>
					 {
						 Console.WriteLine($"Subscription dropped: {reason}");
						 Console.WriteLine("Exception: " + ex.Message);
					 });

					await cancellationToken.WaitHandle.AsTask();

					try
					{
						sub.Stop(); // Does not block.
					}
					catch (Exception)
					{
						// Ignore errors.
					}

					conn.Close();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception while subscribing to stream.");
				throw;
			}
		}

		public async Task<long?> FindLastPositionInStreamAsync(string region, string streamId)
		{
			try
			{
				long? lastPosition = null;

				using (var conn = _connectionFactory.Create(region))
				{
					await conn.ConnectAsync();
					var result = await conn.ReadEventAsync(streamId, StreamPosition.End, false);

					if (result.Status == EventReadStatus.Success)
					{
						if (result.Event.HasValue)
						{
							lastPosition = result.Event.Value.OriginalEventNumber;
						}
					}

					conn.Close();
				}

				return lastPosition;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception while finding end of stream.");
				throw;
			}
		}
	}
}
