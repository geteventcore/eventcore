using EventCore.EventSourcing;
using EventCore.Utilities;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulEventSubscriber
{
	public class ResolutionQueue : IResolutionQueue
	{
		// NOTE: Safe for concurrent operations, expecting multiple subscription listeners
		// to send events to the queue in parallel.

		private readonly int _maxQueueSize;
		private readonly ConcurrentQueue<StreamEvent> _queue = new ConcurrentQueue<StreamEvent>();
		private readonly ManualResetEventSlim _dequeueTrigger = new ManualResetEventSlim(false);

		public ResolutionQueue(int maxQueueSize)
		{
			_maxQueueSize = maxQueueSize;
		}

		public ManualResetEventSlim EnqueueTrigger { get; } = new ManualResetEventSlim(false);

		public async Task EnqueueWithWaitAsync(StreamEvent streamEvent, CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				// Is there room for at least one more in the queue?
				// Note if there are other listeners adding to the queue that we
				// may end up going over the max queue size, but it will never exceed
				// that threshold by more than one so this is acceptable.
				if (_queue.Count < _maxQueueSize)
				{
					_queue.Enqueue(streamEvent);
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

		public StreamEvent TryDequeue()
		{
			StreamEvent streamEvent;
			if(_queue.TryDequeue(out streamEvent)) return streamEvent;
			else return null;
		}
	}
}
