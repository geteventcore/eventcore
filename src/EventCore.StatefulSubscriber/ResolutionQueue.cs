using EventCore.EventSourcing;
using EventCore.Utilities;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public class ResolutionQueue : IResolutionQueue
	{
		// NOTE: Safe for concurrent operations, expecting multiple subscription listeners
		// to send events to the queue in parallel.

		private readonly IQueueAwaiter _awaiter;
		private readonly int _maxQueueSize;
		private readonly ConcurrentQueue<StreamEvent> _queue = new ConcurrentQueue<StreamEvent>();

		// Need these for testing.
		public int QueueCount { get => _queue.Count; }

		public ResolutionQueue(IQueueAwaiter awaiter, int maxQueueSize)
		{
			_awaiter = awaiter;
			_maxQueueSize = maxQueueSize;
		}

		public async Task EnqueueWithWaitAsync(StreamEvent streamEvent, CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				// Is there room for at least one more in the queue?
				// Note if there are other listeners adding to the queue concurrently that we
				// may end up going over the max queue size, but we only ever exceed
				// that threshold by the number of instantaneous concurrent additions, i.e. a few
				// items at most, so this is acceptable.
				if (_queue.Count < _maxQueueSize)
				{
					_queue.Enqueue(streamEvent);
					_awaiter.SetEnqueueSignal();
					return;
				}
				else
				{
					await Task.WhenAny(new Task[] { _awaiter.AwaitDequeueSignalAsync(), cancellationToken.WaitHandle.AsTask() });
				}
			}
		}

		public StreamEvent TryDequeue()
		{
			StreamEvent streamEvent;

			if (_queue.TryDequeue(out streamEvent))
			{
				_awaiter.SetDequeueSignal(); // Signal that we've dequeued an item.
				return streamEvent;
			}

			return null;
		}

		public  Task AwaitEnqueueSignalAsync() => _awaiter.AwaitEnqueueSignalAsync();
	}
}
