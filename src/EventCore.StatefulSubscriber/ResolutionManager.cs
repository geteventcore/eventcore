using EventCore.EventSourcing;
using EventCore.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public class ResolutionManager : IResolutionManager
	{
		private readonly IStandardLogger _logger;
		private readonly IBusinessEventResolver _resolver;
		private readonly IStreamStateRepo _streamStateRepo;
		private readonly IResolutionQueue _resolutionQueue;
		private readonly ISortingManager _sortingManager;

		public ResolutionManager(
			IStandardLogger logger, IBusinessEventResolver resolver, IStreamStateRepo streamStateRepo,
			IResolutionQueue resolutionQueue, ISortingManager sortingManager)
		{
			_logger = logger;
			_resolver = resolver;
			_streamStateRepo = streamStateRepo;
			_resolutionQueue = resolutionQueue;
			_sortingManager = sortingManager;
		}

		// Not thread safe.
		public async Task ManageAsync(CancellationToken cancellationToken)
		{
			try
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					var streamEvent = _resolutionQueue.TryDequeue();
					if (streamEvent != null)
					{
						IBusinessEvent businessEvent = null;

						try
						{
							businessEvent = _resolver.Resolve(streamEvent.EventType, streamEvent.Data);
						}
						catch (Exception ex)
						{
							_logger.LogError(ex, "Exception while resolving business event. Subscription event will be created with null business event.");
						}


						// Stream events may (and will for subscription streams) have links to original events.
						// We want the original event as the core event and the subscription event info as secondary.
						// However, if the event is not a link then we take the event info as both subscription info and original event.
						var streamId = streamEvent.StreamId;
						var position = streamEvent.Position;
						var subscriptionStreamId = streamEvent.StreamId;
						var subscriptionPosition = streamEvent.Position;
						if (streamEvent.IsLink)
						{
							streamId = streamEvent.Link.StreamId;
							position = streamEvent.Link.Position;
						}
						var subscriberEvent = new SubscriberEvent(streamId, position, subscriptionStreamId, subscriptionPosition, businessEvent);

						// Send to the sorting manager.
						await _sortingManager.ReceiveSubscriberEventAsync(subscriberEvent, cancellationToken);
					}
					await Task.WhenAny(new Task[] { _resolutionQueue.AwaitEnqueueSignalAsync(), cancellationToken.WaitHandle.AsTask() });
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception while managing resolution.");
				throw;
			}
		}

		public async Task ReceiveStreamEventAsync(StreamEvent streamEvent, long firstPositionInStream, CancellationToken cancellationToken)
		{
			if ((await IsStreamEventEligibleForResolution(streamEvent, firstPositionInStream)) == ResolutionEligibility.Eligible)
			{
				// Send to the resolution queue when space opens up.
				await _resolutionQueue.EnqueueWithWaitAsync(streamEvent, cancellationToken);
			}
		}

		public async Task<ResolutionEligibility> IsStreamEventEligibleForResolution(StreamEvent streamEvent, long firstPositionInStream)
		{
			var streamState = await _streamStateRepo.LoadStreamStateAsync(streamEvent.StreamId);

			long expectedPosition = firstPositionInStream;
			if (streamState != null)
			{
				if (streamState.HasError) return ResolutionEligibility.UnableStreamHasError; // Ignore events from errored streams.

				if (streamEvent.Position <= streamState.LastAttemptedPosition) return ResolutionEligibility.SkippedAlreadyProcessed; // Skip events already processed.

				if (!_resolver.CanResolve(streamEvent.EventType)) return ResolutionEligibility.UnableToResolveEventType;

				expectedPosition = streamState.LastAttemptedPosition + 1;
			}

			// Sanity check to ensure events arrive sequentially for a given stream.
			if (streamEvent.Position != expectedPosition)
			{
				throw new InvalidOperationException($"Expected sequential event position {expectedPosition} from stream {streamEvent.StreamId} but received {streamEvent.Position}. Unable to continue.");
			}

			return ResolutionEligibility.Eligible;
		}
	}
}
