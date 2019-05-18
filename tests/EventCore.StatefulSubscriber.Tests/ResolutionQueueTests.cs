using EventCore.EventSourcing;
using EventCore.Utilities;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventCore.StatefulSubscriber.Tests
{
	public class ResolutionQueueTests
	{
		[Fact]
		public async Task enqueue_and_dequeue_single_item()
		{
			var cts = new CancellationTokenSource(10000);
			var maxQueueSize = 1;
			var mockQueueAwaiter = new Mock<IQueueAwaiter>();
			var queue = new ResolutionQueue(mockQueueAwaiter.Object, maxQueueSize);
			var streamEvent = new StreamEvent("s", 1, null, "x", new byte[] { });

			await queue.EnqueueWithWaitAsync(streamEvent, cts.Token);
			var dequeuedStreamEvent = queue.TryDequeue();

			Assert.Equal(streamEvent, dequeuedStreamEvent);
		}

		[Fact]
		public async Task set_queue_signals()
		{
			var cts = new CancellationTokenSource(10000);
			var maxQueueSize = 1;
			var mockQueueAwaiter = new Mock<IQueueAwaiter>();
			var queue = new ResolutionQueue(mockQueueAwaiter.Object, maxQueueSize);
			var streamEvent = new StreamEvent("s", 1, null, "x", new byte[] { });

			queue.TryDequeue();

			mockQueueAwaiter.VerifyNoOtherCalls();

			await queue.EnqueueWithWaitAsync(streamEvent, cts.Token);

			mockQueueAwaiter.Verify(x => x.SetEnqueueSignal());
			mockQueueAwaiter.VerifyNoOtherCalls();

			queue.TryDequeue();
			mockQueueAwaiter.Verify(x => x.SetDequeueSignal());
		}

		[Fact]
		public async Task await_enqueue_signal()
		{
			var maxQueueSize = 1;
			var mockQueueAwaiter = new Mock<IQueueAwaiter>();
			var queue = new ResolutionQueue(mockQueueAwaiter.Object, maxQueueSize);

			await queue.AwaitEnqueueSignalAsync();

			mockQueueAwaiter.Verify(x => x.AwaitEnqueueSignalAsync());
		}

		[Fact]
		public async Task honor_max_queue_size()
		{
			var cts = new CancellationTokenSource(10000);
			var maxQueueSize = 2;
			var mockQueueAwaiter = new Mock<IQueueAwaiter>();
			var queue = new ResolutionQueue(mockQueueAwaiter.Object, maxQueueSize);
			var streamEvent1 = new StreamEvent("s", 1, null, "x", new byte[] { });
			var streamEvent2 = new StreamEvent("s", 2, null, "x", new byte[] { });
			var streamEvent3 = new StreamEvent("s", 3, null, "x", new byte[] { });
			var enqueueuSignalSetCount = 0;
			var awaitingDequeueSignal = new ManualResetEventSlim(true);
			var mockDequeueSignal = new ManualResetEventSlim(false);

			mockQueueAwaiter.Setup(x => x.SetEnqueueSignal()).Callback(() => enqueueuSignalSetCount++);
			mockQueueAwaiter.Setup(x => x.AwaitDequeueSignalAsync()).Callback(() => awaitingDequeueSignal.Set()).Returns(mockDequeueSignal.WaitHandle.AsTask());

			await queue.EnqueueWithWaitAsync(streamEvent1, cts.Token);
			await queue.EnqueueWithWaitAsync(streamEvent2, cts.Token);
			var enqueueTask = queue.EnqueueWithWaitAsync(streamEvent3, cts.Token);

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
