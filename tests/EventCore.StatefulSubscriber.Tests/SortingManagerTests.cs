using EventCore.EventSourcing;
using EventCore.Utilities;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventCore.StatefulSubscriber.Tests
{
	public class SortingManagerTests
	{
	
		private class TestException : Exception { }

		[Fact]
		public async Task manage_until_cancelled()
		{
			var cts = new CancellationTokenSource();
			var mockQueue = new Mock<ISortingQueue>();
			var manager = new SortingManager(NullStandardLogger.Instance, mockQueue.Object, null, null);
			var awaitingEnqueueSignal = new ManualResetEventSlim(false);
			var mockEnqueueSignal = new ManualResetEventSlim(false);

			mockQueue.Setup(x => x.TryDequeue(out It.Ref<SubscriberEvent>.IsAny)).Returns(false);
			mockQueue.Setup(x => x.AwaitEnqueueSignalAsync()).Callback(() => awaitingEnqueueSignal.Set()).Returns(mockEnqueueSignal.WaitHandle.AsTask());

			var manageTask = manager.ManageAsync(cts.Token);

			var timeoutToken1 = new CancellationTokenSource(10000).Token;
			await Task.WhenAny(new[] { awaitingEnqueueSignal.WaitHandle.AsTask(), timeoutToken1.WaitHandle.AsTask() });
			if (timeoutToken1.IsCancellationRequested) throw new TimeoutException();

			cts.Cancel();

			var timeoutToken2 = new CancellationTokenSource(10000).Token;
			await Task.WhenAny(new[] { manageTask, timeoutToken2.WaitHandle.AsTask() });
			if (timeoutToken2.IsCancellationRequested) throw new TimeoutException();
		}

		[Fact]
		public async Task rethrow_exception_when_managing()
		{
			var cts = new CancellationTokenSource(10000);
			var ex = new TestException();
			var mockQueue = new Mock<ISortingQueue>();
			var manager = new SortingManager(NullStandardLogger.Instance, mockQueue.Object, null, null);

			mockQueue.Setup(x => x.TryDequeue(out It.Ref<SubscriberEvent>.IsAny)).Throws(ex);

			await Assert.ThrowsAsync<TestException>(() => manager.ManageAsync(cts.Token));
		}

		[Fact]
		public async Task throw_when_parallel_key_empty()
		{
			var cts = new CancellationTokenSource(10000);
			var mockQueue = new Mock<ISortingQueue>();
			var mockSorter = new Mock<ISubscriberEventSorter>();
			var manager = new SortingManager(NullStandardLogger.Instance, mockQueue.Object, mockSorter.Object, null);
			var subscriberEvent = new SubscriberEvent(null, null, 0, null, null);
			var parallelKey = ""; // Must be empty (or null).

			mockQueue.Setup(x => x.TryDequeue(out subscriberEvent)).Returns(true);
			mockSorter.Setup(x => x.SortSubscriberEventToParallelKey(subscriberEvent)).Returns(parallelKey);

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
			var subscriberEvent = new SubscriberEvent(null, null, 0, null, null);
			var parallelKey = "x";

			mockQueue.Setup(x => x.TryDequeue(out subscriberEvent)).Returns(true);
			mockSorter.Setup(x => x.SortSubscriberEventToParallelKey(subscriberEvent)).Returns(parallelKey);
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
			var subscriberEvent = new SubscriberEvent(null, null, 0, null, null);
			var parallelKey = "x";
			var awaitingEnqueueSignal = new ManualResetEventSlim(false);
			var mockEnqueueSignal = new ManualResetEventSlim(false);

			mockQueue.Setup(x => x.TryDequeue(out It.Ref<SubscriberEvent>.IsAny)).Returns(false);
			mockSorter.Setup(x => x.SortSubscriberEventToParallelKey(subscriberEvent)).Returns(parallelKey);
			mockQueue.Setup(x => x.AwaitEnqueueSignalAsync()).Callback(() => awaitingEnqueueSignal.Set()).Returns(mockEnqueueSignal.WaitHandle.AsTask());

			var manageTask = manager.ManageAsync(cts.Token);

			await Task.WhenAny(new[] { awaitingEnqueueSignal.WaitHandle.AsTask(), cts.Token.WaitHandle.AsTask() });
			if (cts.IsCancellationRequested) throw new TimeoutException();

			cts.Cancel();
		}

		[Fact]
		public async Task receive_and_enqueue_subscriber_event()
		{
			var cts = new CancellationTokenSource(10000);
			var mockQueue = new Mock<ISortingQueue>();
			var manager = new SortingManager(NullStandardLogger.Instance, mockQueue.Object, null, null);
			var subscriberEvent = new SubscriberEvent(null, null, 0, null, null);

			mockQueue.Setup(x => x.EnqueueWithWaitAsync(It.IsAny<SubscriberEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

			await manager.ReceiveSubscriberEventAsync(subscriberEvent, cts.Token);

			mockQueue.Verify(x => x.EnqueueWithWaitAsync(subscriberEvent, cts.Token));
		}
	}
}
