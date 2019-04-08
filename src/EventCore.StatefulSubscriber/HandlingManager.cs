using EventCore.EventSourcing;
using EventCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public class HandlingManager : IHandlingManager
	{
		private readonly IStandardLogger _logger;
		private readonly IBusinessEventResolver _resolver;
		private readonly IStreamStateRepo _streamStateRepo;
		private readonly IHandlingQueue _handlingQueue;
		private readonly ISubscriberEventHandler _handler;

		// Key is parallel key used to group events into parallel execution.
		private readonly Dictionary<string, Task> _handlerTasks = new Dictionary<string, Task>(StringComparer.OrdinalIgnoreCase);

		private readonly SemaphoreSlim _throttle; // Used to throttle parallel executions.
		private readonly ManualResetEventSlim _handlerCompletionTrigger = new ManualResetEventSlim(false);

		public Dictionary<string, Task> HandlerTasks { get => _handlerTasks; }
		public SemaphoreSlim Throttle { get => _throttle; }
		public ManualResetEventSlim HandlerCompletionTrigger { get => _handlerCompletionTrigger; }

		public HandlingManager(
			IStandardLogger logger, IBusinessEventResolver resolver, IStreamStateRepo streamStateRepo,
			IHandlingQueue handlingQueue, ISubscriberEventHandler handler,
			int maxParallelHandlerExecutions)
		{
			_logger = logger;
			_resolver = resolver;
			_streamStateRepo = streamStateRepo;
			_handlingQueue = handlingQueue;
			_handler = handler;

			_throttle = new SemaphoreSlim(maxParallelHandlerExecutions, maxParallelHandlerExecutions);
		}

		// Not thread safe.
		public async Task ManageAsync(CancellationToken cancellationToken)
		{
			try
			{
				var handlerCompletionTrigger = new ManualResetEventSlim(false);

				// Dictionary key is the case INsensitive parallel key used to group parallel executions.
				var parallelTasks = new Dictionary<string, Task>(StringComparer.OrdinalIgnoreCase);

				while (!cancellationToken.IsCancellationRequested)
				{
					while (_handlingQueue.IsEventsAvailable)
					{
						_handlerCompletionTrigger.Reset();

						// Clean up finished tasks.
						foreach (var key in _handlerTasks.Where(kvp => kvp.Value.IsCanceled || kvp.Value.IsCompleted || kvp.Value.IsFaulted).Select(kvp => kvp.Key).ToList())
						{
							_handlerTasks.Remove(key);
						}

						var item = _handlingQueue.TryDequeue(parallelTasks.Keys.ToList());
						if (item != null)
						{
							await RunHandlerTaskAsync(item.ParallelKey, item.SubscriberEvent, cancellationToken);
						}
						else
						{
							// Events are available but none in a parallel group that is not already executing.
							// Wait for a new event to arrive or for an event handler to complete.
							await Task.WhenAny(new Task[] {
								handlerCompletionTrigger.WaitHandle.AsTask(),
								_handlingQueue.EnqueueTrigger.WaitHandle.AsTask(),
								cancellationToken.WaitHandle.AsTask() });
						}
						if (cancellationToken.IsCancellationRequested) break;
					}

					if (cancellationToken.IsCancellationRequested) break;

					await Task.WhenAny(new Task[] { _handlingQueue.EnqueueTrigger.WaitHandle.AsTask(), cancellationToken.WaitHandle.AsTask() });
					_handlingQueue.EnqueueTrigger.Reset();
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

		public async Task RunHandlerTaskAsync(string parallelKey, SubscriberEvent subscriberEvent, CancellationToken cancellationToken)
		{
			var state = await _streamStateRepo.LoadStreamStateAsync(subscriberEvent.StreamId);
			if (state.HasError)
			{
				// When a stream has an error there may already be subscriber events in the
				// handling queue that have made it past the first check for errored stream state
				// prior to deserialization. If that's the case we just ignore the event.
				return;
			}

			await _throttle.WaitAsync(cancellationToken);

			if (cancellationToken.IsCancellationRequested) return;

			_handlerTasks.Add(
				parallelKey,
				Task.Run(async () =>
				{
					try
					{
						await RunHandlerAsync(subscriberEvent, cancellationToken);
					}
					finally
					{
						_throttle.Release(); // Make room for other handlers.
						_handlerCompletionTrigger.Set();
					}
				}
				)
			);
		}

		public async Task RunHandlerAsync(SubscriberEvent subscriberEvent, CancellationToken cancellationToken)
		{
			try
			{
				await _handler.HandleAsync(subscriberEvent, cancellationToken);
				await _streamStateRepo.SaveStreamStateAsync(subscriberEvent.StreamId, subscriberEvent.Position, false);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception while handling event. Stream will be halted.");

				// Save errored stream state.
				await _streamStateRepo.SaveStreamStateAsync(subscriberEvent.StreamId, subscriberEvent.Position, true);
			}
		}
	}
}
