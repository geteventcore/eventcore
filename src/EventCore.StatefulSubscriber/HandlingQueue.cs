using EventCore.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public class HandlingQueue : IHandlingQueue
	{
		// NOTE: This implementation expects that only one process will enqueue
		// and only one process will dequeue, with both enqueue/dequeue occuring simultaneously.

		private readonly IQueueAwaiter _awaiter;
		private readonly int _maxQueuesSharedSize;

		private readonly ConcurrentDictionary<string, ConcurrentQueue<SubscriberEvent>> _queues =
			new ConcurrentDictionary<string, ConcurrentQueue<SubscriberEvent>>(StringComparer.OrdinalIgnoreCase);

		public bool IsEventsAvailable => _queues.Any(kvp => kvp.Value.Count > 0);

		// Need these for testing.
		public int QueueCount { get => _queues.Sum(kvp => kvp.Value.Count); }

		public HandlingQueue(IQueueAwaiter awaiter, int maxQueuesSharedSize)
		{
			_awaiter = awaiter;
			_maxQueuesSharedSize = maxQueuesSharedSize;
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
				if (_queues.Sum(x => x.Value.Count) < _maxQueuesSharedSize)
				{
					ConcurrentQueue<SubscriberEvent> queue;
					if (!_queues.TryGetValue(parallelKey, out queue))
					{
						queue = new ConcurrentQueue<SubscriberEvent>();
						_queues.TryAdd(parallelKey, queue);
					}

					queue.Enqueue(subscriberEvent);
					_awaiter.SetEnqueueSignal(); // Signal that we've enqueued a new item.

					return;
				}
				else
				{
					await Task.WhenAny(new Task[] { _awaiter.AwaitDequeueSignalAsync(), cancellationToken.WaitHandle.AsTask() });
				}
			}
		}

		public HandlingQueueItem TryDequeue(IList<string> filterOutParallelKeys)
		{
			var key = _queues
				.Where(kvp => kvp.Value.Count > 0)
				.Select(kvp => kvp.Key)
				.Where(x => !filterOutParallelKeys.Contains(x, StringComparer.OrdinalIgnoreCase))
				.FirstOrDefault();

			if (string.IsNullOrEmpty(key)) return null;

			ConcurrentQueue<SubscriberEvent> queue;
			if (_queues.TryGetValue(key, out queue))
			{
				if (queue.Count > 0)
				{
					SubscriberEvent subscriberEvent;
					if (queue.TryDequeue(out subscriberEvent))
					{
						_awaiter.SetDequeueSignal(); // Signal that we've dequeued an item.
						return new HandlingQueueItem(key, subscriberEvent);
					}
				}
			}
			return null;
		}

		public Task AwaitEnqueueSignalAsync() => _awaiter.AwaitEnqueueSignalAsync();
	}
}
