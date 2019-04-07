﻿using EventCore.Utilities;
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

		private readonly int _maxSharedQueueSize;

		private readonly ConcurrentDictionary<string, Queue<SubscriberEvent>> _queues =
			new ConcurrentDictionary<string, Queue<SubscriberEvent>>(StringComparer.OrdinalIgnoreCase);

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
				// Is there room for at least one more among the total of all queue counts?
				if (_queues.Sum(x => x.Value.Count) < _maxSharedQueueSize)
				{
					// Clean up empty queues.
					foreach (var key in _queues.Where(kvp => kvp.Value.Count == 0).Select(kvp => kvp.Key).ToList())
					{
						Queue<SubscriberEvent> _;
						_queues.TryRemove(key, out _);
					}

					Queue<SubscriberEvent> queue;
					if (!_queues.TryGetValue(parallelKey, out queue))
					{
						queue = new Queue<SubscriberEvent>();
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

			Queue<SubscriberEvent> queue;
			if (_queues.TryGetValue(key, out queue))
			{
				if (queue.Count > 0)
				{
					var subscriberEvent = queue.Dequeue();
					_dequeueTrigger.Set(); // Signal that we've dequeued an item.
					return new HandlingQueueItem(key, subscriberEvent);
				}
			}
			return null;
		}
	}
}
