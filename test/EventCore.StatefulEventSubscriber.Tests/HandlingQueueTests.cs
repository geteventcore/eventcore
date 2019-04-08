using EventCore.EventSourcing;
using EventCore.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventCore.StatefulEventSubscriber.Tests
{
	public class HandlingQueueTests
	{
		[Fact]
		public async Task enqueue_and_dequeue_items()
		{
			var cts = new CancellationTokenSource(3000);
			var queue = new HandlingQueue(2);
			var parallelKey1 = "pk1";
			var parallelKey2 = "pk2";
			var subscriberEvent1 = new SubscriberEvent("s", 1, new BusinessEvent(BusinessMetadata.Empty));
			var subscriberEvent2 = new SubscriberEvent("s", 2, new BusinessEvent(BusinessMetadata.Empty));

			await queue.EnqueueWithWaitAsync(parallelKey1, subscriberEvent1, cts.Token);
			await queue.EnqueueWithWaitAsync(parallelKey2, subscriberEvent2, cts.Token);

			if(cts.IsCancellationRequested) throw new TimeoutException();

			var dequeuedItem1 = queue.TryDequeue(new string[] { parallelKey2 });
			var dequeuedItem2 = queue.TryDequeue(new string[] { parallelKey1 });

			Assert.Equal(parallelKey1, dequeuedItem1.ParallelKey);
			Assert.Equal(subscriberEvent1, dequeuedItem1.SubscriberEvent);

			Assert.Equal(parallelKey2, dequeuedItem2.ParallelKey);
			Assert.Equal(subscriberEvent2, dequeuedItem2.SubscriberEvent);
		}

		[Fact]
		public async Task enqueue_and_not_dequeue_with_matching_parallel_key()
		{
			var cts = new CancellationTokenSource(3000);
			var queue = new HandlingQueue(1);
			var subscriberEvent = new SubscriberEvent("s", 1, new BusinessEvent(BusinessMetadata.Empty));
			var parallelKey = "pk";
			var notInParallelKeys = new List<string>() { parallelKey };

			await queue.EnqueueWithWaitAsync(parallelKey, subscriberEvent, cts.Token);
			var dequeuedItem = queue.TryDequeue(notInParallelKeys);

			Assert.Null(dequeuedItem);
		}

		[Fact]
		public async Task honor_max_queue_size()
		{
			var cts = new CancellationTokenSource(3000);
			var maxQueueSize = 2;
			var queue = new HandlingQueue(maxQueueSize);
			var parallelKey1 = "pk1";
			var parallelKey2 = "pk2";
			var parallelKey3 = "pk3";
			var subscriberEvent1 = new SubscriberEvent("s", 1, new BusinessEvent(BusinessMetadata.Empty));
			var subscriberEvent2 = new SubscriberEvent("s", 1, new BusinessEvent(BusinessMetadata.Empty));
			var subscriberEvent3 = new SubscriberEvent("s", 1, new BusinessEvent(BusinessMetadata.Empty));

			await queue.EnqueueWithWaitAsync(parallelKey1, subscriberEvent1, cts.Token);
			await queue.EnqueueWithWaitAsync(parallelKey2, subscriberEvent2, cts.Token);

			var enqueueTask = queue.EnqueueWithWaitAsync(parallelKey3, subscriberEvent3, cts.Token);

			await Task.WhenAny(new[] { queue.EnqueueIsWaitingSignal.WaitHandle.AsTask(), cts.Token.WaitHandle.AsTask() });
			if (cts.Token.IsCancellationRequested) throw new TimeoutException();

			Assert.Equal(maxQueueSize, queue.QueueCount);

			Assert.NotNull(queue.TryDequeue(new string[] { }));
			await Task.WhenAny(new[] { enqueueTask, cts.Token.WaitHandle.AsTask() });
			if (cts.Token.IsCancellationRequested) throw new TimeoutException();

			Assert.Equal(maxQueueSize, queue.QueueCount);
		}
	}
}
