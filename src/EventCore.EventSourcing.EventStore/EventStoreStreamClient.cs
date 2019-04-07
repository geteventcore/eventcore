﻿using EventCore.Utilities;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.EventSourcing.EventStore
{
	public class EventStoreStreamClient : IStreamClient
	{
		// NOTE: Stream ids are case SENSITIVE in GY's Event Store.
		// Haven't figured out how to disable this yet.

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

		public async Task<CommitResult> CommitEventsToStreamAsync(string regionId, string streamId, long? expectedLastPosition, IEnumerable<CommitEvent> events)
		{
			if (!StreamIdBuilder.ValidateStreamIdFragment(streamId))
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

				using (var conn = _connectionFactory.Create(regionId))
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
		}

		private IEnumerable<EventData> MapCommitEvents(IEnumerable<CommitEvent> events)
		{
			foreach (var e in events)
			{
				yield return new EventData(Guid.NewGuid(), e.EventType, true, e.Data, null);
			}
		}

		public async Task LoadStreamEventsAsync(string regionId, string streamId, long fromPosition, Func<StreamEvent, CancellationToken, Task> receiverAsync, CancellationToken cancellationToken)
		{
			if (fromPosition < FirstPositionInStream)
			{
				throw new ArgumentException("Invalid position.");
			}

			try
			{
				StreamEventsSlice currentSlice;
				long nextSliceStart = StreamPosition.Start;

				using (var conn = _connectionFactory.Create(regionId))
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
							await ReceiveResolvedEventAsync(receiverAsync, resolvedEvent, cancellationToken);
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

		public async Task SubscribeToStreamAsync(string regionId, string streamId, long fromPosition, Func<StreamEvent, CancellationToken, Task> receiverAsync, CancellationToken cancellationToken)
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

				using (var conn = _connectionFactory.Create(regionId))
				{
					await conn.ConnectAsync();

					var sub = conn.SubscribeToStreamFrom(
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

		private async Task ReceiveResolvedEventAsync(Func<StreamEvent, CancellationToken, Task> receiverAsync, ResolvedEvent resolvedEvent, CancellationToken cancellationToken)
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
				await receiverAsync(streamEvent, cancellationToken);
			}
		}

		public async Task<long?> FindLastPositionInStreamAsync(string regionId, string streamId)
		{
			try
			{
				long? lastPosition = null;

				using (var conn = _connectionFactory.Create(regionId))
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
