using EventCore.EventSourcing;
using EventCore.Utilities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public class DeserializationQueue : IDeserializationQueue
	{
		// NOTE: This implementation expects that only one process will enqueue
		// and only one process will dequeue, with both enqueue/dequeue occuring simultaneously.

		private readonly int _maxQueueSize;
		private readonly Queue<StreamEvent> _queue = new Queue<StreamEvent>();
		private readonly ManualResetEventSlim _dequeueTrigger = new ManualResetEventSlim(false);

		public DeserializationQueue(int maxQueueSize)
		{
			_maxQueueSize = maxQueueSize;
		}

		public ManualResetEventSlim EnqueueTrigger { get; } = new ManualResetEventSlim(false);

		public async Task EnqueueWithWaitAsync(StreamEvent streamEvent, CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				// Is there room for at least one more in the queue?
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
			if(_queue.Count > 0) return _queue.Dequeue();
			else return null;
		}
	}
}
