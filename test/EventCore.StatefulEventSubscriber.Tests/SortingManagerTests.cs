using EventCore.EventSourcing;
using EventCore.Utilities;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventCore.StatefulEventSubscriber.Tests
{
	public class SortingManagerTests
	{
		private class TestException : Exception { }

		[Fact]
		public async Task rethrow_exception_when_managing()
		{
			var cts = new CancellationTokenSource(10000);
			var ex = new TestException();
			var mockQueue = new Mock<IResolutionQueue>();
			var manager = new ResolutionManager(NullStandardLogger.Instance, null, null, mockQueue.Object, null);

			mockQueue.Setup(x => x.TryDequeue()).Throws(ex);

			await Assert.ThrowsAsync<TestException>(() => manager.ManageAsync(cts.Token));
		}

		[Fact]
		public async Task throw_when_parallel_key_empty()
		{
			var cts = new CancellationTokenSource(10000);
			var mockQueue = new Mock<ISortingQueue>();
			var mockSorter = new Mock<ISubscriberEventSorter>();
			var manager = new SortingManager(NullStandardLogger.Instance, mockQueue.Object, mockSorter.Object, null);
			var streamId = "s";
			var position = 1;
			var businessEvent = new BusinessEvent(BusinessMetadata.Empty);
			var subscriberEvent = new SubscriberEvent(streamId, position, businessEvent);
			var parallelKey = ""; // Must be empty (or null).

			mockQueue.Setup(x => x.TryDequeue()).Returns(subscriberEvent);
			mockSorter.Setup(x => x.SortToParallelKey(subscriberEvent)).Returns(parallelKey);

			await Assert.ThrowsAsync<ArgumentException>(() => manager.ManageAsync(cts.Token));
		}

		[Fact]
		public async Task sort_subscriber_event_and_send_to_handling_manager()
		{
			var cts = new CancellationTokenSource(10000);
			var mockQueue = new Mock<ISortingQueue>();
			var mockSorter = new Mock<ISubscriberEventSorter>();
			var mockHandlingManager = new Mock<IHandlingManager>();
			var manager = new SortingManager(NullStandardLogger.Instance, mockQueue.Object, mockSorter.Object, mockHandlingManager.Object);
			var enqueueTrigger = new ManualResetEventSlim(false);
			var streamId = "s";
			var position = 1;
			var businessEvent = new BusinessEvent(BusinessMetadata.Empty);
			var subscriberEvent = new SubscriberEvent(streamId, position, businessEvent);
			var parallelKey = "x"; // This can be anything.

			mockQueue.Setup(x => x.TryDequeue()).Returns(subscriberEvent);
			mockSorter.Setup(x => x.SortToParallelKey(subscriberEvent)).Returns(parallelKey);
			mockQueue.Setup(x => x.EnqueueTrigger).Returns(enqueueTrigger);
			mockHandlingManager
				.Setup(x => x.ReceiveSubscriberEventAsync(It.IsAny<string>(), It.IsAny<SubscriberEvent>(), It.IsAny<CancellationToken>()))
				.Callback(() => cts.Cancel())
				.Returns(Task.CompletedTask);

			await manager.ManageAsync(cts.Token);

			mockHandlingManager.Verify(x => x.ReceiveSubscriberEventAsync(parallelKey, subscriberEvent, cts.Token));
		}

		[Fact]
		public async Task wait_for_enqueue_when_managing_and_no_events_in_queue()
		{
			var cts = new CancellationTokenSource(10000);
			var mockQueue = new Mock<ISortingQueue>();
			var mockSorter = new Mock<ISubscriberEventSorter>();
			var mockHandlingManager = new Mock<IHandlingManager>();
			var manager = new SortingManager(NullStandardLogger.Instance, mockQueue.Object, mockSorter.Object, mockHandlingManager.Object);
			var enqueueTrigger = new ManualResetEventSlim(false);
			var manageIsWaitingSignal = new ManualResetEventSlim(false);
			var streamId = "s";
			var position = 1;
			var businessEvent = new BusinessEvent(BusinessMetadata.Empty);
			var subscriberEvent = new SubscriberEvent(streamId, position, businessEvent);
			var parallelKey = "x"; // This can be anything.

			mockQueue.Setup(x => x.TryDequeue()).Returns(subscriberEvent);
			mockSorter.Setup(x => x.SortToParallelKey(subscriberEvent)).Returns(parallelKey);
			mockQueue.Setup(x => x.EnqueueTrigger).Callback(() => manageIsWaitingSignal.Set()).Returns(enqueueTrigger);

			var manageTask = manager.ManageAsync(cts.Token);

			await Task.WhenAny(new[] { manageIsWaitingSignal.WaitHandle.AsTask(), cts.Token.WaitHandle.AsTask() });
			if (cts.IsCancellationRequested) throw new TimeoutException();

			cts.Cancel();

			await Task.WhenAny(new[] { manageTask, new CancellationTokenSource(1000).Token.WaitHandle.AsTask() });

			Assert.True(manageTask.IsCompletedSuccessfully);
		}

		[Fact]
		public async Task receive_and_enqueue_subscriber_event()
		{
			var cts = new CancellationTokenSource(10000);
			var mockQueue = new Mock<ISortingQueue>();
			var streamId = "s";
			var position = 1;
			var manager = new SortingManager(NullStandardLogger.Instance, mockQueue.Object, null, null);
			var businessEvent = new BusinessEvent(BusinessMetadata.Empty);
			var subscriberEvent = new SubscriberEvent(streamId, position, businessEvent);

			mockQueue.Setup(x => x.EnqueueWithWaitAsync(It.IsAny<SubscriberEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

			await manager.ReceiveSubscriberEventAsync(subscriberEvent, cts.Token);

			mockQueue.Verify(x => x.EnqueueWithWaitAsync(subscriberEvent, cts.Token));
		}
	}
}
