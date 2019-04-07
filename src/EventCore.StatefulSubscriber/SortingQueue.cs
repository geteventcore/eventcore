using EventCore.Utilities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public class SortingQueue : ISortingQueue
	{
		// NOTE: This implementation expects that only one process will enqueue
		// and only one process will dequeue, with both enqueue/dequeue occuring simultaneously.

		private readonly int _maxQueueSize;
		private readonly Queue<SubscriberEvent> _queue = new Queue<SubscriberEvent>();
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
			if(_queue.Count > 0) return _queue.Dequeue();
			else return null;
		}
	}
}
