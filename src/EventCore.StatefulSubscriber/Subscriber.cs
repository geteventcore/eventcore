using EventCore.EventSourcing;
using EventCore.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public class Subscriber : ISubscriber
	{
		// General flow of execution...
		// 1. Stream events received from stream client listening on multiple regions.
		// 2. Stream events rejected if event type is not resolvable.
		// 3. Stream events enqueued for deserialization/resolution.
		// 4. Stream event deserialization attempted resulting in stateful subscriber event.
		// 5. Subscriber event enqueued for sorting into parallel handler executions.
		// 6. Handlers called in parallel by pulling events in order off of handling queue.
		//
		// Handling queue will be grouped into parallel keys, where each group has guaranteed
		// in-order events per stream.
		// (Multiple streams may be interleaved in one group, but at the stream level order is guaranteed.)

		private readonly IGenericLogger _logger;
		private readonly IStreamClient _streamClient;
		private readonly IStreamStateRepo _streamStateRepo;
		private readonly SubscriberOptions _options;

		private bool _isSubscribing = false;

		private readonly IDeserializationQueue _deserializationQueue;
		private readonly ISortingQueue _sortingQueue;
		private readonly IHandlingQueue _handlingQueue;

		// private readonly Dictionary<string, ConcurrentQueue<StatefulSubscriberEvent>> _handlingQueues =
		// 	new Dictionary<string, ConcurrentQueue<StatefulSubscriberEvent>>(StringComparer.Ordinal);
		// private readonly SemaphoreSlim _handlingQueueMutex = new SemaphoreSlim(1, 1);
		// private readonly ManualResetEventSlim _handlingQueuesTrigger = new ManualResetEventSlim(false);

		public Subscriber(
			IGenericLogger logger, IStreamClient streamClient, IStreamStateRepo streamStateRepo,
			IDeserializationQueue deserializationQueue, ISortingQueue sortingQueue,
			IHandlingQueue handlingQueue,
			SubscriberOptions options)
		{
			_logger = logger;
			_streamClient = streamClient;
			_streamStateRepo = streamStateRepo;
			_deserializationQueue = deserializationQueue;
			_sortingQueue = sortingQueue;
			_handlingQueue = handlingQueue;
			_options = options;
		}

		public async Task ResetStreamErrorStatesAsync()
		{
			if (_isSubscribing)
				throw new InvalidOperationException("Can't reset while subscribing.");

			await Task.Delay(10);
		}

		public async Task ResetAllStatesAsync()
		{
			if (_isSubscribing)
				throw new InvalidOperationException("Can't reset while subscribing.");

			await Task.Delay(10);
		}

		public async Task SubscribeAsync(IBusinessEventResolver resolver, Func<SubscriberEvent, string> sorter, Func<SubscriberEvent, CancellationToken, Task> handlerAsync, CancellationToken cancellationToken)
		{
			if (_isSubscribing) throw new InvalidOperationException("Already subscribing.");

			_isSubscribing = true;
			_logger.LogInformation("Stateful subscriber started.");

			var tasks = new List<Task>();

			// One subscription listener for each region.
			foreach (var regionId in _options.RegionIds.Distinct(StringComparer.OrdinalIgnoreCase))
			{
				tasks.Add(ManageRegionalSubscriptionAsync(_logger, resolver, _streamClient, regionId, _options.StreamId, _streamStateRepo, _deserializationQueue, cancellationToken));
			}

			tasks.Add(ManageDeserializationAsync(_logger, _deserializationQueue, _sortingQueue, resolver, cancellationToken));
			tasks.Add(ManageSortingAsync(_logger, _sortingQueue, _handlingQueue, sorter, cancellationToken));
			tasks.Add(ManageHandlingAsync(_logger, _handlingQueue, _streamStateRepo, handlerAsync, _options.MaxParallelHandlerExecutions, cancellationToken));
			tasks.Add(cancellationToken.WaitHandle.AsTask());

			await Task.WhenAny(tasks);

			_logger.LogInformation("Stateful subscriber stopped.");
			_isSubscribing = false;
		}

		public static async Task ManageRegionalSubscriptionAsync(IGenericLogger logger, IBusinessEventResolver resolver, IStreamClient streamClient, string regionId, string streamId, IStreamStateRepo streamStateRepo, IDeserializationQueue deserializationQueue, CancellationToken cancellationToken)
		{
			try
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					// Subscription starts from first position in stream.
					// Streams states will be read to skip previously processed events.
					var listenerTask = streamClient.SubscribeToStreamAsync(
						regionId, streamId, streamClient.FirstPositionInStream,
						async (se, ct) =>
						{
							if ((await IsStreamEventEligibleForDeserialization(resolver, streamClient, streamId, streamStateRepo, se)) == DeserializationEligibility.Eligible)
							{
								// Send to the deserialization queue when space opens up.
								await deserializationQueue.EnqueueWithWaitAsync(se, cancellationToken);
							}
						},
						cancellationToken
					);
					await Task.WhenAny(new Task[] { cancellationToken.WaitHandle.AsTask() });
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Exception while managing regional subscription.");
				throw;
			}
		}

		public static async Task<DeserializationEligibility> IsStreamEventEligibleForDeserialization(IBusinessEventResolver resolver, IStreamClient streamClient, string streamId, IStreamStateRepo repo, StreamEvent streamEvent)
		{
			var streamState = await repo.LoadStreamStateAsync(streamId);

			long expectedPosition = streamClient.FirstPositionInStream;
			if (streamState != null)
			{
				if (streamState.HasError) return DeserializationEligibility.UnableStreamHasError; // Ignore events from errored streams.

				if (streamEvent.Position <= streamState.LastProcessedPosition.Value) return DeserializationEligibility.SkippedAlreadyProcessed; // Skip events already processed.

				if (!resolver.CanResolve(streamEvent.EventType)) return DeserializationEligibility.UnableToResolveEventType;

				if (streamState.LastProcessedPosition.HasValue)
					expectedPosition = streamState.LastProcessedPosition.Value + 1;
			}

			// Sanity check to ensure events arrive sequentially for a given stream.
			if (streamEvent.Position != expectedPosition)
			{
				throw new InvalidOperationException($"Expected sequential event position {expectedPosition} from stream {streamEvent.StreamId} but received {streamEvent.Position}. Unable to continue.");
			}

			return DeserializationEligibility.Eligible;
		}

		public static async Task ManageDeserializationAsync(IGenericLogger logger, IDeserializationQueue deserializationQueue, ISortingQueue sortingQueue, IBusinessEventResolver resolver, CancellationToken cancellationToken)
		{
			try
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					var streamEvent = deserializationQueue.TryDequeue();
					if (streamEvent != null)
					{
						var businessEvent = resolver.ResolveEvent(streamEvent.EventType, streamEvent.Data);
						var subscriberEvent = new SubscriberEvent(streamEvent.StreamId, streamEvent.Position, businessEvent);

						// Send to the sorting queue when space opens up.
						await sortingQueue.EnqueueWithWaitAsync(subscriberEvent, cancellationToken);
					}
					await Task.WhenAny(new Task[] { deserializationQueue.EnqueueTrigger.WaitHandle.AsTask(), cancellationToken.WaitHandle.AsTask() });
					deserializationQueue.EnqueueTrigger.Reset();
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Exception while managing deserialization.");
				throw;
			}
		}

		public static async Task ManageSortingAsync(IGenericLogger logger, ISortingQueue sortingQueue, IHandlingQueue handlingQueue, Func<SubscriberEvent, string> sorter, CancellationToken cancellationToken)
		{
			try
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					var subscriberEvent = sortingQueue.TryDequeue();
					if (subscriberEvent != null)
					{
						// Expecting a case INsensitive key used to group executions.
						var parallelKey = sorter(subscriberEvent);

						if (string.IsNullOrEmpty(parallelKey))
							throw new ArgumentException("Parallel key can't be null or empty.");

						// Send to the handling queue when space opens up.
						await handlingQueue.EnqueueWithWaitAsync(parallelKey, subscriberEvent, cancellationToken);
					}
					await Task.WhenAny(new Task[] { sortingQueue.EnqueueTrigger.WaitHandle.AsTask(), cancellationToken.WaitHandle.AsTask() });
					sortingQueue.EnqueueTrigger.Reset();
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Exception while managing sorting.");
				throw;
			}
		}

		public static async Task ManageHandlingAsync(IGenericLogger logger, IHandlingQueue handlingQueue, IStreamStateRepo repo, Func<SubscriberEvent, CancellationToken, Task> handlerAsync, int maxParallelHandlerExecutions, CancellationToken cancellationToken)
		{
			try
			{
				// Used to throttle parallel executions.
				var throttle = new SemaphoreSlim(maxParallelHandlerExecutions, maxParallelHandlerExecutions);
				var handlerCompletionTrigger = new ManualResetEventSlim(false);

				// Dictionary key is the case INsensitive parallel key used to group parallel executions.
				var parallelTasks = new Dictionary<string, Task>(StringComparer.OrdinalIgnoreCase);

				while (!cancellationToken.IsCancellationRequested)
				{
					while (handlingQueue.IsEventsAvailable)
					{
						handlerCompletionTrigger.Reset();

						// Clean up finished tasks.
						foreach (var key in parallelTasks.Where(kvp => kvp.Value.IsCanceled || kvp.Value.IsCompleted || kvp.Value.IsFaulted).Select(kvp => kvp.Key).ToList())
						{
							parallelTasks.Remove(key);
						}

						var item = handlingQueue.TryDequeue(parallelTasks.Keys.ToList());
						if (item != null)
						{
							await throttle.WaitAsync(cancellationToken);
							
							if (cancellationToken.IsCancellationRequested) break;

							parallelTasks.Add(
								item.ParallelKey,
								Task.Run(async () =>
								{
									try
									{
										await RunHandlerAsync(logger, handlerAsync, repo, item.SubscriberEvent, cancellationToken);
									}
									finally
									{
										throttle.Release(); // Make room for other handlers.
										handlerCompletionTrigger.Set();
									}
								}
								)
							);
						}
						else
						{
							// Events are available but none in a parallel group that is not already executing.
							// Wait for a new event to arrive or for an event handler to complete.
							await Task.WhenAny(new Task[] {
								handlerCompletionTrigger.WaitHandle.AsTask(),
								handlingQueue.EnqueueTrigger.WaitHandle.AsTask(),
								cancellationToken.WaitHandle.AsTask() });

							if (cancellationToken.IsCancellationRequested) break;
						}
					}

					if (cancellationToken.IsCancellationRequested) break;

					await Task.WhenAny(new Task[] { handlingQueue.EnqueueTrigger.WaitHandle.AsTask(), cancellationToken.WaitHandle.AsTask() });
					handlingQueue.EnqueueTrigger.Reset();
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Exception while managing handler execution.");
				throw;
			}
		}

		public static async Task WaitForSemaphoreOrManualResetAsync(SemaphoreSlim semaphore, ManualResetEventSlim mre, CancellationToken cancellationToken)
		{
			var mutexCts = new CancellationTokenSource();
			try
			{
				await Task.WhenAny(new Task[] { semaphore.WaitAsync(mutexCts.Token), mre.WaitHandle.AsTask(), cancellationToken.WaitHandle.AsTask() });
				if (cancellationToken.IsCancellationRequested)
				{
					mutexCts.Cancel(); // Mutex needs a separate token source to ensure we don't leave it waiting.
				}
			}
			finally
			{
				if (!mre.IsSet && !cancellationToken.IsCancellationRequested)
					semaphore.Release();

				if (mre.IsSet)
					mre.Reset();
			}
		}

		public static async Task WaitForManualResetAsync(ManualResetEventSlim mre, CancellationToken cancellationToken)
		{
			await Task.WhenAny(new Task[] { mre.WaitHandle.AsTask(), cancellationToken.WaitHandle.AsTask() });
			mre.Reset();
		}

		public static async Task RunHandlerAsync(IGenericLogger logger, Func<SubscriberEvent, CancellationToken, Task> handlerAsync, IStreamStateRepo streamStateRepo, SubscriberEvent subscriberEvent, CancellationToken cancellationToken)
		{
			try
			{
				await handlerAsync(subscriberEvent, cancellationToken);

				// Save stream state...
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Exception while handling event. Stream will be halted.");

				// Save errored stream state...
			}
		}
	}
}
