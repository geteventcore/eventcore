using EventCore.EventSourcing;
using EventCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulEventSubscriber
{
	public class HandlingManager : IHandlingManager
	{
		private readonly IStandardLogger _logger;
		private readonly IHandlingManagerAwaiter _awaiter;
		private readonly IStreamStateRepo _streamStateRepo;
		private readonly IHandlingQueue _handlingQueue;
		private readonly IHandlingManagerHandlerRunner _handlerRunner;
		private readonly IHandlingManagerTaskCollection _handlerTasks;

		public HandlingManager(
			IStandardLogger logger, IHandlingManagerAwaiter awaiter, IStreamStateRepo streamStateRepo,
			IHandlingQueue handlingQueue, IHandlingManagerHandlerRunner handlerRunner, IHandlingManagerTaskCollection handlerTasks)
		{
			_logger = logger;
			_awaiter = awaiter;
			_streamStateRepo = streamStateRepo;
			_handlingQueue = handlingQueue;
			_handlerRunner = handlerRunner;
			_handlerTasks = handlerTasks;
		}

		// Not thread safe.
		public async Task ManageAsync(CancellationToken cancellationToken)
		{
			try
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					while (_handlingQueue.IsEventsAvailable)
					{
						_awaiter.ResetHandlerCompletionSignal();

						// Clean up finished tasks.
						_handlerTasks.PurgeFinishedTasks();

						var item = _handlingQueue.TryDequeue(_handlerTasks.Keys.ToList());
						if (item != null)
						{
							// When a stream has an error there may already be subscriber events in the
							// handling queue that have made it past the first check for errored stream state
							// prior to deserialization. If that's the case we just ignore the event.
							// Note this may represent an opportunity for optimization - if loading from disk
							// is a bottleneck then we need some other way to receive feedback from the handlers when
							// a stream is in an error state.
							var state = await _streamStateRepo.LoadStreamStateAsync(item.SubscriberEvent.StreamId);
							if (!state.HasError)
							{
								await Task.WhenAny(new[] { _awaiter.AwaitThrottleAsync(), cancellationToken.WaitHandle.AsTask() });

								if (!cancellationToken.IsCancellationRequested)
								{
									_handlerTasks.Add(item.ParallelKey, _handlerRunner.TryRunHandlerAsync(item.ParallelKey, item.SubscriberEvent, cancellationToken));
								}
							}
						}
						else
						{
							// Events are available but none in a parallel group that is not already executing.
							// Wait for a new event to arrive or for an event handler to complete.
							await Task.WhenAny(new Task[] {
								_awaiter.AwaitHandlerCompletionSignalAsync(),
								_handlingQueue.AwaitEnqueueSignalAsync(),
								cancellationToken.WaitHandle.AsTask() });
						}

						if (cancellationToken.IsCancellationRequested) break;
					}

					if (cancellationToken.IsCancellationRequested) break;

					await Task.WhenAny(new Task[] { _handlingQueue.AwaitEnqueueSignalAsync(), cancellationToken.WaitHandle.AsTask() });
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception while managing handler execution.");
				throw;
			}
		}

		public async Task ReceiveSubscriberEventAsync(string parallelKey, SubscriberEvent subscriberEvent, CancellationToken cancellationToken)
		{
			// Enqueue when space available.
			await _handlingQueue.EnqueueWithWaitAsync(parallelKey, subscriberEvent, cancellationToken);
		}
	}
}
