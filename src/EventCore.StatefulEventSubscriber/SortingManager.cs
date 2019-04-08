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
		private readonly IBusinessEventResolver _resolver;
		private readonly IStreamStateRepo _streamStateRepo;
		private readonly ISortingQueue _sortingQueue;
		private readonly ISubscriberEventSorter _sorter;
		private readonly IHandlingManager _handlingManager;

		public SortingManager(
			IStandardLogger logger, IBusinessEventResolver resolver, IStreamStateRepo streamStateRepo,
			ISortingQueue sortingQueue, ISubscriberEventSorter sorter, IHandlingManager handlingManager)
		{
			_logger = logger;
			_resolver = resolver;
			_streamStateRepo = streamStateRepo;
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

						// Send to the handling queue.
						await _handlingManager.ReceiveSubscriberEventAsync(parallelKey, subscriberEvent, cancellationToken);
					}
					await Task.WhenAny(new Task[] { _sortingQueue.EnqueueTrigger.WaitHandle.AsTask(), cancellationToken.WaitHandle.AsTask() });
					_sortingQueue.EnqueueTrigger.Reset();
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
