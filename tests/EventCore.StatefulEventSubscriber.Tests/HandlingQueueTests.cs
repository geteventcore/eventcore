using EventCore.EventSourcing;
using EventCore.Utilities;
using Moq;
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
		public async Task enqueue_and_dequeue_items_with_correct_parallel_keys()
		{
			var cts = new CancellationTokenSource(10000);
			var mockQueueAwaiter = new Mock<IQueueAwaiter>();
			var maxQueueSize = 2;
			var queue = new HandlingQueue(mockQueueAwaiter.Object, maxQueueSize);
			var parallelKey1 = "pk1";
			var parallelKey2 = "pk2";
			var subscriberEvent1 = new SubscriberEvent(null, 0, null);
			var subscriberEvent2 = new SubscriberEvent(null, 0, null);

			Assert.False(queue.IsEventsAvailable);

			await queue.EnqueueWithWaitAsync(parallelKey1, subscriberEvent1, cts.Token);
			await queue.EnqueueWithWaitAsync(parallelKey2, subscriberEvent2, cts.Token);

			if (cts.IsCancellationRequested) throw new TimeoutException();

			Assert.True(queue.IsEventsAvailable);

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
			var cts = new CancellationTokenSource(10000);
			var mockQueueAwaiter = new Mock<IQueueAwaiter>();
			var queue = new HandlingQueue(mockQueueAwaiter.Object, 1);
			var subscriberEvent = new SubscriberEvent(null, 0, null);
			var parallelKey = "pk";
			var notInParallelKeys = new List<string>() { parallelKey };

			await queue.EnqueueWithWaitAsync(parallelKey, subscriberEvent, cts.Token);
			var dequeuedItem = queue.TryDequeue(notInParallelKeys);

			Assert.Null(dequeuedItem);
		}

		[Fact]
		public async Task set_queue_signals()
		{
			var cts = new CancellationTokenSource(10000);
			var maxQueueSize = 1;
			var mockQueueAwaiter = new Mock<IQueueAwaiter>();
			var queue = new HandlingQueue(mockQueueAwaiter.Object, maxQueueSize);
			var subscriberEvent = new SubscriberEvent(null, 0, null);
			var parallelKey = "pk";

			queue.TryDequeue(new string[] { });

			mockQueueAwaiter.VerifyNoOtherCalls();

			await queue.EnqueueWithWaitAsync(parallelKey, subscriberEvent, cts.Token);

			mockQueueAwaiter.Verify(x => x.SetEnqueueSignal());
			mockQueueAwaiter.VerifyNoOtherCalls();

			queue.TryDequeue(new string[] { });
			mockQueueAwaiter.Verify(x => x.SetDequeueSignal());
		}

		[Fact]
		public async Task await_enqueue_signal()
		{
			var mockQueueAwaiter = new Mock<IQueueAwaiter>();
			var queue = new HandlingQueue(mockQueueAwaiter.Object, 1);

			await queue.AwaitEnqueueSignalAsync();

			mockQueueAwaiter.Verify(x => x.AwaitEnqueueSignalAsync());
		}

		[Fact]
		public async Task honor_max_queue_size()
		{
			var cts = new CancellationTokenSource(10000);
			var maxQueueSize = 2;
			var mockQueueAwaiter = new Mock<IQueueAwaiter>();
			var queue = new HandlingQueue(mockQueueAwaiter.Object, maxQueueSize);
			var parallelKey1 = "pk1";
			var parallelKey2 = "pk2";
			var parallelKey3 = "pk3";
			var subscriberEvent1 = new SubscriberEvent(null, 0, null);
			var subscriberEvent2 = new SubscriberEvent(null, 0, null);
			var subscriberEvent3 = new SubscriberEvent(null, 0, null);
			var enqueueuSignalSetCount = 0;
			var awaitingDequeueSignal = new ManualResetEventSlim(true);
			var mockDequeueSignal = new ManualResetEventSlim(false);

			mockQueueAwaiter.Setup(x => x.SetEnqueueSignal()).Callback(() => enqueueuSignalSetCount++);
			mockQueueAwaiter.Setup(x => x.AwaitDequeueSignalAsync()).Callback(() => awaitingDequeueSignal.Set()).Returns(mockDequeueSignal.WaitHandle.AsTask());

			await queue.EnqueueWithWaitAsync(parallelKey1, subscriberEvent1, cts.Token);
			await queue.EnqueueWithWaitAsync(parallelKey2, subscriberEvent2, cts.Token);
			var enqueueTask = queue.EnqueueWithWaitAsync(parallelKey3, subscriberEvent3, cts.Token);

			await Task.WhenAny(new[] { awaitingDequeueSignal.WaitHandle.AsTask(), cts.Token.WaitHandle.AsTask() });
			if (cts.Token.IsCancellationRequested) throw new TimeoutException();

			Assert.Equal(2, enqueueuSignalSetCount);
			Assert.Equal(maxQueueSize, queue.QueueCount);

			queue.TryDequeue(new string[] { });
			Assert.Equal(maxQueueSize - 1, queue.QueueCount);

			mockDequeueSignal.Set();

			await Task.WhenAny(new[] { enqueueTask, cts.Token.WaitHandle.AsTask() });
			if (cts.Token.IsCancellationRequested) throw new TimeoutException();

			Assert.Equal(maxQueueSize, queue.QueueCount);
		}
	}
}
