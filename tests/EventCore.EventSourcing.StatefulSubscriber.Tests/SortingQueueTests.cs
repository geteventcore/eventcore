using EventCore.EventSourcing;
using EventCore.Utilities;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventCore.EventSourcing.StatefulSubscriber.Tests
{
	public class SortingQueueTests
	{
		private class TestBusinessEvent : BusinessEvent
		{
			public TestBusinessEvent(BusinessEventMetadata metadata) : base(metadata) { }
		}

		[Fact]
		public async Task enqueue_and_dequeue_single_item()
		{
			var cts = new CancellationTokenSource(10000);
			var maxQueueSize = 1;
			var mockQueueAwaiter = new Mock<IQueueAwaiter>();
			var queue = new SortingQueue(mockQueueAwaiter.Object, maxQueueSize);
			var subscriberEvent = new SubscriberEvent("s", 1, new TestBusinessEvent(BusinessEventMetadata.Empty));

			await queue.EnqueueWithWaitAsync(subscriberEvent, cts.Token);
			var dequeuedSubscriberEvent = queue.TryDequeue();

			Assert.Equal(subscriberEvent, dequeuedSubscriberEvent);
		}

		[Fact]
		public async Task set_queue_signals()
		{
			var cts = new CancellationTokenSource(10000);
			var maxQueueSize = 1;
			var mockQueueAwaiter = new Mock<IQueueAwaiter>();
			var queue = new SortingQueue(mockQueueAwaiter.Object, maxQueueSize);
			var subscriberEvent = new SubscriberEvent("s", 1, new TestBusinessEvent(BusinessEventMetadata.Empty));

			queue.TryDequeue();

			mockQueueAwaiter.VerifyNoOtherCalls();

			await queue.EnqueueWithWaitAsync(subscriberEvent, cts.Token);

			mockQueueAwaiter.Verify(x => x.SetEnqueueSignal());
			mockQueueAwaiter.VerifyNoOtherCalls();

			queue.TryDequeue();
			mockQueueAwaiter.Verify(x => x.SetDequeueSignal());
		}

		[Fact]
		public async Task await_enqueue_signal()
		{
			var mockQueueAwaiter = new Mock<IQueueAwaiter>();
			var queue = new SortingQueue(mockQueueAwaiter.Object, 1);

			await queue.AwaitEnqueueSignalAsync();

			mockQueueAwaiter.Verify(x => x.AwaitEnqueueSignalAsync());
		}

		[Fact]
		public async Task honor_max_queue_size()
		{
			var cts = new CancellationTokenSource(10000);
			var maxQueueSize = 2;
			var mockQueueAwaiter = new Mock<IQueueAwaiter>();
			var queue = new SortingQueue(mockQueueAwaiter.Object, maxQueueSize);
			var subscriberEvent1 = new SubscriberEvent("s", 1, new TestBusinessEvent(BusinessEventMetadata.Empty));
			var subscriberEvent2 = new SubscriberEvent("s", 1, new TestBusinessEvent(BusinessEventMetadata.Empty));
			var subscriberEvent3 = new SubscriberEvent("s", 1, new TestBusinessEvent(BusinessEventMetadata.Empty));
			var enqueueuSignalSetCount = 0;
			var awaitingDequeueSignal = new ManualResetEventSlim(true);
			var mockDequeueSignal = new ManualResetEventSlim(false);

			mockQueueAwaiter.Setup(x => x.SetEnqueueSignal()).Callback(() => enqueueuSignalSetCount++);
			mockQueueAwaiter.Setup(x => x.AwaitDequeueSignalAsync()).Callback(() => awaitingDequeueSignal.Set()).Returns(mockDequeueSignal.WaitHandle.AsTask());

			await queue.EnqueueWithWaitAsync(subscriberEvent1, cts.Token);
			await queue.EnqueueWithWaitAsync(subscriberEvent2, cts.Token);
			var enqueueTask = queue.EnqueueWithWaitAsync(subscriberEvent3, cts.Token);

			await Task.WhenAny(new[] { awaitingDequeueSignal.WaitHandle.AsTask(), cts.Token.WaitHandle.AsTask() });
			if (cts.Token.IsCancellationRequested) throw new TimeoutException();

			Assert.Equal(2, enqueueuSignalSetCount);
			Assert.Equal(maxQueueSize, queue.QueueCount);

			queue.TryDequeue();
			Assert.Equal(maxQueueSize - 1, queue.QueueCount);

			mockDequeueSignal.Set();

			await Task.WhenAny(new[] { enqueueTask, cts.Token.WaitHandle.AsTask() });
			if (cts.Token.IsCancellationRequested) throw new TimeoutException();

			Assert.Equal(maxQueueSize, queue.QueueCount);
		}
	}
}
