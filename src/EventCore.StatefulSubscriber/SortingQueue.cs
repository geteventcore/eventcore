using EventCore.Utilities;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public class SortingQueue : ISortingQueue
	{
		// NOTE: This implementation expects that only one process will enqueue
		// and only one process will dequeue, with both enqueue/dequeue occuring simultaneously.

		private readonly IQueueAwaiter _awaiter;
		private readonly int _maxQueueSize;
		private readonly ConcurrentQueue<SubscriberEvent> _queue = new ConcurrentQueue<SubscriberEvent>();

		// Need these for testing.
		public int QueueCount { get => _queue.Count; }

		public SortingQueue(IQueueAwaiter awaiter, int maxQueueSize)
		{
			_awaiter = awaiter;
			_maxQueueSize = maxQueueSize;
		}

		public async Task EnqueueWithWaitAsync(SubscriberEvent subscriberEvent, CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				// Is there room for at least one more in the queue?
				if (_queue.Count < _maxQueueSize)
				{
					_queue.Enqueue(subscriberEvent);
					_awaiter.SetEnqueueSignal();
					return;
				}
				else
				{
					await Task.WhenAny(new Task[] { _awaiter.AwaitDequeueSignalAsync(), cancellationToken.WaitHandle.AsTask() });
				}
			}
		}

		public bool TryDequeue(out SubscriberEvent subscriberEvent)
		{
			if (_queue.TryDequeue(out subscriberEvent))
			{
				_awaiter.SetDequeueSignal(); // Signal that we've dequeued an item.
				return true;
			}

			return false;
		}

		public Task AwaitEnqueueSignalAsync() => _awaiter.AwaitEnqueueSignalAsync();
	}
}
