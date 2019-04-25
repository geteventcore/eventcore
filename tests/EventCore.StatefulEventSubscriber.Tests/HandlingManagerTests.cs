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
	public class HandlingManagerTests
	{
		private class TestBusinessEvent : IBusinessEvent {}

		private class TestException : Exception { }

		[Fact]
		public async Task manage_until_cancelled()
		{
			var cts = new CancellationTokenSource();
			var mockQueue = new Mock<IHandlingQueue>();
			var manager = new HandlingManager(NullStandardLogger.Instance, null, null, mockQueue.Object, null, null);
			var awaitingEnqueueSignal = new ManualResetEventSlim(false);
			var mockEnqueueSignal = new ManualResetEventSlim(false);

			mockQueue.Setup(x => x.IsEventsAvailable).Returns(false);
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
			var mockQueue = new Mock<IHandlingQueue>();
			var manager = new HandlingManager(NullStandardLogger.Instance, null, null, mockQueue.Object, null, null);

			mockQueue.Setup(x => x.IsEventsAvailable).Throws(new TestException());

			await Assert.ThrowsAsync<TestException>(() => manager.ManageAsync(cts.Token));
		}

		[Fact]
		public async Task await_both_enqueue_and_handler_completion_when_managing_and_events_in_queue_but_not_parallelable()
		{
			var cts = new CancellationTokenSource(10000);
			var mockAwaiter = new Mock<IHandlingManagerAwaiter>();
			var mockTaskCollection = new Mock<IHandlingManagerTaskCollection>();
			var mockQueue = new Mock<IHandlingQueue>();
			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
			var mockHandlerRunner = new Mock<IHandlingManagerHandlerRunner>();
			var manager = new HandlingManager(NullStandardLogger.Instance, mockAwaiter.Object, mockStreamStateRepo.Object, mockQueue.Object, mockHandlerRunner.Object, mockTaskCollection.Object);
			var awaitingEnqueueSignal = new ManualResetEventSlim(false);
			var awaitingCompletionSignal = new ManualResetEventSlim(false);
			var mockCompletionAndEnqueueSignal = new ManualResetEventSlim(false);

			mockQueue.Setup(x => x.IsEventsAvailable).Returns(true);
			mockQueue.Setup(x => x.TryDequeue(new string[] { })).Returns((HandlingQueueItem)null);
			mockQueue.Setup(x => x.AwaitEnqueueSignalAsync()).Callback(() => awaitingEnqueueSignal.Set()).Returns(mockCompletionAndEnqueueSignal.WaitHandle.AsTask());
			mockAwaiter.Setup(x => x.AwaitHandlerCompletionSignalAsync()).Callback(() => awaitingCompletionSignal.Set()).Returns(mockCompletionAndEnqueueSignal.WaitHandle.AsTask());

			var manageTask = manager.ManageAsync(cts.Token);

			await Task.WhenAny(new[] {
				Task.WhenAll(new[] { awaitingEnqueueSignal.WaitHandle.AsTask(), awaitingCompletionSignal.WaitHandle.AsTask() }),
				cts.Token.WaitHandle.AsTask() });
			if (cts.IsCancellationRequested) throw new TimeoutException();

			cts.Cancel();
			mockCompletionAndEnqueueSignal.Set();
		}

		[Fact]
		public async Task wait_for_enqueue_when_managing_and_no_events_in_queue()
		{
			var cts = new CancellationTokenSource(10000);
			var mockQueue = new Mock<IHandlingQueue>();
			var manager = new HandlingManager(NullStandardLogger.Instance, null, null, mockQueue.Object, null, null);
			var subscriberEvent = new SubscriberEvent(null, 0, null);
			var parallelKey = "x";
			var awaitingEnqueueSignal = new ManualResetEventSlim(false);
			var mockEnqueueSignal = new ManualResetEventSlim(false);

			mockQueue.Setup(x => x.TryDequeue(It.IsAny<IList<string>>())).Returns(new HandlingQueueItem(parallelKey, subscriberEvent));
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
			var mockQueue = new Mock<IHandlingQueue>();
			var manager = new HandlingManager(NullStandardLogger.Instance, null, null, mockQueue.Object, null, null);
			var subscriberEvent = new SubscriberEvent(null, 0, null);
			var parallelKey = "x";

			mockQueue.Setup(x => x.EnqueueWithWaitAsync(It.IsAny<string>(), It.IsAny<SubscriberEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

			await manager.ReceiveSubscriberEventAsync(parallelKey, subscriberEvent, cts.Token);

			mockQueue.Verify(x => x.EnqueueWithWaitAsync(parallelKey, subscriberEvent, cts.Token));
		}

		[Fact]
		public async Task purge_finished_handler_tasks()
		{
			var cts = new CancellationTokenSource();
			var mockAwaiter = new Mock<IHandlingManagerAwaiter>();
			var mockTaskCollection = new Mock<IHandlingManagerTaskCollection>();
			var mockQueue = new Mock<IHandlingQueue>();
			var manager = new HandlingManager(NullStandardLogger.Instance, mockAwaiter.Object, null, mockQueue.Object, null, mockTaskCollection.Object);

			mockQueue.Setup(x => x.IsEventsAvailable).Returns(true);
			mockQueue.Setup(x => x.TryDequeue(It.IsAny<IList<string>>())).Callback(() => cts.Cancel()).Returns((HandlingQueueItem)null);

			var timeoutToken = new CancellationTokenSource(10000).Token;
			await Task.WhenAny(new[] { manager.ManageAsync(cts.Token), timeoutToken.WaitHandle.AsTask() });
			cts.Cancel();
			if (timeoutToken.IsCancellationRequested) throw new TimeoutException();

			mockTaskCollection.Verify(x => x.PurgeFinishedTasks());
		}

		[Fact]
		public async Task reset_handler_completion_signal()
		{
			var cts = new CancellationTokenSource();
			var mockAwaiter = new Mock<IHandlingManagerAwaiter>();
			var mockTaskCollection = new Mock<IHandlingManagerTaskCollection>();
			var mockQueue = new Mock<IHandlingQueue>();
			var manager = new HandlingManager(NullStandardLogger.Instance, mockAwaiter.Object, null, mockQueue.Object, null, mockTaskCollection.Object);

			mockQueue.Setup(x => x.IsEventsAvailable).Returns(true);
			mockQueue.Setup(x => x.TryDequeue(It.IsAny<IList<string>>())).Callback(() => cts.Cancel()).Returns((HandlingQueueItem)null);

			var timeoutToken = new CancellationTokenSource(10000).Token;
			await Task.WhenAny(new[] { manager.ManageAsync(cts.Token), timeoutToken.WaitHandle.AsTask() });
			cts.Cancel();
			if (timeoutToken.IsCancellationRequested) throw new TimeoutException();

			mockAwaiter.Verify(x => x.ResetHandlerCompletionSignal());
		}

		[Fact]
		public async Task await_throttle_and_run_handler_when_stream_state_does_not_have_error()
		{
			var cts = new CancellationTokenSource();
			var mockAwaiter = new Mock<IHandlingManagerAwaiter>();
			var mockTaskCollection = new Mock<IHandlingManagerTaskCollection>();
			var mockQueue = new Mock<IHandlingQueue>();
			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
			var mockHandlerRunner = new Mock<IHandlingManagerHandlerRunner>();
			var manager = new HandlingManager(NullStandardLogger.Instance, mockAwaiter.Object, mockStreamStateRepo.Object, mockQueue.Object, mockHandlerRunner.Object, mockTaskCollection.Object);
			var businessEvent = new TestBusinessEvent();
			var subscriberEvent = new SubscriberEvent(null, 0, null);
			var parallelKey = "x";
			var queueItem = new HandlingQueueItem(parallelKey, subscriberEvent);
			var streamState = new StreamState(0, false); // Does NOT have error.
			var awaitingThrottleSignal = new ManualResetEventSlim(false);

			mockQueue.Setup(x => x.IsEventsAvailable).Returns(true);
			mockQueue.Setup(x => x.TryDequeue(It.IsAny<IList<string>>())).Returns(queueItem);
			mockQueue.Setup(x => x.AwaitEnqueueSignalAsync()).Callback(() => cts.Cancel()).Returns(Task.CompletedTask); // Ensures manager can't get into an infinite loop.
			mockStreamStateRepo.Setup(x => x.LoadStreamStateAsync(It.IsAny<string>())).ReturnsAsync(streamState);
			mockAwaiter.Setup(x => x.AwaitThrottleAsync()).Callback(() => awaitingThrottleSignal.Set()).Returns(Task.CompletedTask);
			mockHandlerRunner.Setup(x => x.TryRunHandlerAsync(It.IsAny<SubscriberEvent>(), It.IsAny<CancellationToken>())).Callback(() => cts.Cancel()).Returns(Task.CompletedTask);

			var manageTask = manager.ManageAsync(cts.Token);

			var timeoutToken1 = new CancellationTokenSource(10000).Token;
			await Task.WhenAny(new[] { awaitingThrottleSignal.WaitHandle.AsTask(), timeoutToken1.WaitHandle.AsTask() });
			if (timeoutToken1.IsCancellationRequested)
			{
				cts.Cancel();
				throw new TimeoutException();
			}

			var timeoutToken2 = new CancellationTokenSource(10000).Token;
			await Task.WhenAny(new[] { manageTask, timeoutToken2.WaitHandle.AsTask() });
			cts.Cancel();
			if (timeoutToken2.IsCancellationRequested) throw new TimeoutException();

			mockHandlerRunner.Verify(x => x.TryRunHandlerAsync(subscriberEvent, cts.Token));
		}

		[Fact]
		public async Task not_run_handler_when_stream_state_has_error()
		{
			var cts = new CancellationTokenSource();
			var mockAwaiter = new Mock<IHandlingManagerAwaiter>();
			var mockTaskCollection = new Mock<IHandlingManagerTaskCollection>();
			var mockQueue = new Mock<IHandlingQueue>();
			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
			var mockHandlerRunner = new Mock<IHandlingManagerHandlerRunner>();
			var manager = new HandlingManager(NullStandardLogger.Instance, mockAwaiter.Object, mockStreamStateRepo.Object, mockQueue.Object, mockHandlerRunner.Object, mockTaskCollection.Object);
			var subscriberEvent = new SubscriberEvent(null, 0, null);
			var parallelKey = "x";
			var queueItem = new HandlingQueueItem(parallelKey, subscriberEvent);
			var streamState = new StreamState(0, true); // Has error.

			mockQueue.Setup(x => x.IsEventsAvailable).Returns(true);
			mockQueue.Setup(x => x.TryDequeue(It.IsAny<IList<string>>())).Returns(queueItem);
			mockStreamStateRepo.Setup(x => x.LoadStreamStateAsync(It.IsAny<string>())).Callback(() => cts.Cancel()).ReturnsAsync(streamState);

			var timeoutToken = new CancellationTokenSource(10000).Token;
			await Task.WhenAny(new[] { manager.ManageAsync(cts.Token), timeoutToken.WaitHandle.AsTask() });
			cts.Cancel();
			if (timeoutToken.IsCancellationRequested) throw new TimeoutException();

			mockAwaiter.Verify(x => x.ResetHandlerCompletionSignal());
			mockAwaiter.VerifyNoOtherCalls();
		}
	}
}
