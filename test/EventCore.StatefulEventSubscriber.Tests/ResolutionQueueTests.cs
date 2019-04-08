using EventCore.EventSourcing;
using EventCore.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventCore.StatefulEventSubscriber.Tests
{
	public class ResolutionQueueTests
	{
		[Fact]
		public async Task enqueue_and_dequeue_single_item()
		{
			var cts = new CancellationTokenSource(3000);
			var queue = new ResolutionQueue(1);
			var streamEvent = new StreamEvent("s", 1, null, "x", new byte[] { });

			await queue.EnqueueWithWaitAsync(streamEvent, cts.Token);
			var dequeuedStreamEvent = queue.TryDequeue();

			Assert.Equal(streamEvent, dequeuedStreamEvent);
		}

		[Fact]
		public async Task honor_max_queue_size()
		{
			var cts = new CancellationTokenSource(3000);
			var maxQueueSize = 2;
			var queue = new ResolutionQueue(maxQueueSize);
			var streamEvent1 = new StreamEvent("s", 1, null, "x", new byte[] { });
			var streamEvent2 = new StreamEvent("s", 2, null, "x", new byte[] { });
			var streamEvent3 = new StreamEvent("s", 3, null, "x", new byte[] { });

			await queue.EnqueueWithWaitAsync(streamEvent1, cts.Token);
			await queue.EnqueueWithWaitAsync(streamEvent2, cts.Token);

			var enqueueTask = queue.EnqueueWithWaitAsync(streamEvent3, cts.Token);

			await Task.WhenAny(new[] { queue.EnqueueIsWaitingSignal.WaitHandle.AsTask(), cts.Token.WaitHandle.AsTask() });
			if (cts.Token.IsCancellationRequested) throw new TimeoutException();

			Assert.Equal(maxQueueSize, queue.QueueCount);

			Assert.NotNull(queue.TryDequeue());
			await Task.WhenAny(new[] { enqueueTask, cts.Token.WaitHandle.AsTask() });
			if (cts.Token.IsCancellationRequested) throw new TimeoutException();

			Assert.Equal(maxQueueSize, queue.QueueCount);
		}
	}
}
