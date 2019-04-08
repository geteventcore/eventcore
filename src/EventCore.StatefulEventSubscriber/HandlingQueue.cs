using EventCore.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulEventSubscriber
{
	public class HandlingQueue : IHandlingQueue
	{
		// NOTE: This implementation expects that only one process will enqueue
		// and only one process will dequeue, with both enqueue/dequeue occuring simultaneously.

		private readonly int _maxSharedQueueSize;

		private readonly ConcurrentDictionary<string, ConcurrentQueue<SubscriberEvent>> _queues =
			new ConcurrentDictionary<string, ConcurrentQueue<SubscriberEvent>>(StringComparer.OrdinalIgnoreCase);

		private readonly ManualResetEventSlim _dequeueTrigger = new ManualResetEventSlim(false);

		public ManualResetEventSlim EnqueueTrigger { get; } = new ManualResetEventSlim(false);

		public bool IsEventsAvailable => _queues.Any(x => x.Value.Count > 0);

		public HandlingQueue(int maxSharedQueueSize)
		{
			_maxSharedQueueSize = maxSharedQueueSize;
		}

		public async Task EnqueueWithWaitAsync(string parallelKey, SubscriberEvent subscriberEvent, CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				// Clean up empty queues.
				foreach (var key in _queues.Where(kvp => kvp.Value.Count == 0).Select(kvp => kvp.Key).ToList())
				{
					ConcurrentQueue<SubscriberEvent> _;
					_queues.TryRemove(key, out _);
				}

				// Is there room for at least one more among the total of all queue counts?
				if (_queues.Sum(x => x.Value.Count) < _maxSharedQueueSize)
				{
					ConcurrentQueue<SubscriberEvent> queue;
					if (!_queues.TryGetValue(parallelKey, out queue))
					{
						queue = new ConcurrentQueue<SubscriberEvent>();
						_queues.TryAdd(parallelKey, queue);
					}

					queue.Enqueue(subscriberEvent);
					EnqueueTrigger.Set(); // Signal that we've enqueued a new item.
				}
				else
				{
					await Task.WhenAny(new Task[] { _dequeueTrigger.WaitHandle.AsTask(), cancellationToken.WaitHandle.AsTask() });
					_dequeueTrigger.Reset();
				}
			}
		}

		public HandlingQueueItem TryDequeue(IList<string> filterOutParallelKeys)
		{
			var key = _queues.Keys.Where(x => !filterOutParallelKeys.Contains(x, StringComparer.OrdinalIgnoreCase)).FirstOrDefault();
			if (string.IsNullOrEmpty(key)) return null;

			ConcurrentQueue<SubscriberEvent> queue;
			if (_queues.TryGetValue(key, out queue))
			{
				if (queue.Count > 0)
				{
					SubscriberEvent subscriberEvent;
					if (queue.TryDequeue(out subscriberEvent))
					{
						_dequeueTrigger.Set(); // Signal that we've dequeued an item.
						return new HandlingQueueItem(key, subscriberEvent);
					}
				}
			}
			return null;
		}
	}
}
