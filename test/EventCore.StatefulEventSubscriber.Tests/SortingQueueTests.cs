using EventCore.EventSourcing;
using EventCore.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventCore.StatefulEventSubscriber.Tests
{
	public class SortingQueueTests
	{
		[Fact]
		public async Task enqueue_and_dequeue_single_item()
		{
			var cts = new CancellationTokenSource(3000);
			var queue = new SortingQueue(1);
			var subscriberEvent = new SubscriberEvent("s", 1, new BusinessEvent(BusinessMetadata.Empty));

			await queue.EnqueueWithWaitAsync(subscriberEvent, cts.Token);
			var dequeuedSubscriberEvent = queue.TryDequeue();

			Assert.Equal(subscriberEvent, dequeuedSubscriberEvent);
		}

		[Fact]
		public async Task honor_max_queue_size()
		{
			var cts = new CancellationTokenSource(3000);
			var maxQueueSize = 2;
			var queue = new SortingQueue(maxQueueSize);
			var subscriberEvent1 = new SubscriberEvent("s", 1, new BusinessEvent(BusinessMetadata.Empty));
			var subscriberEvent2 = new SubscriberEvent("s", 1, new BusinessEvent(BusinessMetadata.Empty));
			var subscriberEvent3 = new SubscriberEvent("s", 1, new BusinessEvent(BusinessMetadata.Empty));

			await queue.EnqueueWithWaitAsync(subscriberEvent1, cts.Token);
			await queue.EnqueueWithWaitAsync(subscriberEvent2, cts.Token);

			var enqueueTask = queue.EnqueueWithWaitAsync(subscriberEvent3, cts.Token);

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
