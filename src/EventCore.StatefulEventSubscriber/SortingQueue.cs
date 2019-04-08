using EventCore.Utilities;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulEventSubscriber
{
	public class SortingQueue : ISortingQueue
	{
		// NOTE: This implementation expects that only one process will enqueue
		// and only one process will dequeue, with both enqueue/dequeue occuring simultaneously.

		private readonly int _maxQueueSize;
		private readonly ConcurrentQueue<SubscriberEvent> _queue = new ConcurrentQueue<SubscriberEvent>();
		private readonly ManualResetEventSlim _dequeueTrigger = new ManualResetEventSlim(false);

		public SortingQueue(int maxQueueSize)
		{
			_maxQueueSize = maxQueueSize;
		}

		public ManualResetEventSlim EnqueueTrigger { get; } = new ManualResetEventSlim(false);

		public async Task EnqueueWithWaitAsync(SubscriberEvent subscriberEvent, CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				// Is there room for at least one more in the queue?
				if (_queue.Count < _maxQueueSize)
				{
					_queue.Enqueue(subscriberEvent);
					EnqueueTrigger.Set();
					return;
				}
				else
				{
					await Task.WhenAny(new Task[] { _dequeueTrigger.WaitHandle.AsTask(), cancellationToken.WaitHandle.AsTask() });
					_dequeueTrigger.Reset();
				}
			}
		}

		public SubscriberEvent TryDequeue()
		{
			SubscriberEvent subscriberEvent;
			if(_queue.TryDequeue(out subscriberEvent)) return subscriberEvent;
			else return null;
		}
	}
}
