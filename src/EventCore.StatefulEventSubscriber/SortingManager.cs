using EventCore.EventSourcing;
using EventCore.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulEventSubscriber
{
	public class SortingManager : ISortingManager
	{
		private readonly IStandardLogger _logger;
		private readonly ISortingQueue _sortingQueue;
		private readonly ISubscriberEventSorter _sorter;
		private readonly IHandlingManager _handlingManager;

		public SortingManager(IStandardLogger logger, ISortingQueue sortingQueue, ISubscriberEventSorter sorter, IHandlingManager handlingManager)
		{
			_logger = logger;
			_sortingQueue = sortingQueue;
			_sorter = sorter;
			_handlingManager = handlingManager;
		}

		// Not thread safe.
		public async Task ManageAsync(CancellationToken cancellationToken)
		{
			try
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					var subscriberEvent = _sortingQueue.TryDequeue();
					if (subscriberEvent != null)
					{
						// Expecting a case INsensitive key used to group executions.
						var parallelKey = _sorter.SortToParallelKey(subscriberEvent);

						if (string.IsNullOrEmpty(parallelKey))
							throw new ArgumentException("Parallel key can't be null or empty.");

						// Send to the handling manager.
						await _handlingManager.ReceiveSubscriberEventAsync(parallelKey, subscriberEvent, cancellationToken);
					}
					await Task.WhenAny(new Task[] { _sortingQueue.AwaitEnqueueSignalAsync(), cancellationToken.WaitHandle.AsTask() });
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception while managing sorting.");
				throw;
			}
		}

		public async Task ReceiveSubscriberEventAsync(SubscriberEvent subscriberEvent, CancellationToken cancellationToken)
		{
			// Enque when space opens up.
			await _sortingQueue.EnqueueWithWaitAsync(subscriberEvent, cancellationToken);
		}
	}
}
