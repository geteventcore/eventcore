// using EventCore.EventSourcing;
// using EventCore.Utilities;
// using Moq;
// using System;
// using System.Threading;
// using System.Threading.Tasks;
// using Xunit;

// namespace EventCore.StatefulEventSubscriber.Tests
// {
// 	public class HandlingManagerTests
// 	{
// 		private class TestException : Exception { }

// 		[Fact]
// 		public async Task rethrow_exception_when_managing()
// 		{
// 			var cts = new CancellationTokenSource(10000);
// 			var ex = new TestException();
// 			var mockQueue = new Mock<IHandlingQueue>();
// 			var manager = new HandlingManager(NullStandardLogger.Instance, null, mockQueue.Object, null, 1);

// 			mockQueue.Setup(x => x.IsEventsAvailable).Throws(new TestException());

// 			await Assert.ThrowsAsync<TestException>(() => manager.ManageAsync(cts.Token));
// 		}

// 		[Fact]
// 		public async Task manage_and_handle_when_events_in_queue()
// 		{
// 			var cts = new CancellationTokenSource(10000);
// 			var mockQueue = new Mock<IHandlingQueue>();
// 			var maxParallelExcutions = 2;
// 			var manager = new HandlingManager(NullStandardLogger.Instance, null, mockQueue.Object, null, maxParallelExcutions);
// 			var streamIdA = "sA";
// 			var streamIdB = "sB";
// 			var startPositionA = 1;
// 			var startPositionB = 20;
// 			var businessEvent = new BusinessEvent(BusinessMetadata.Empty);
// 			var subscriberEventA1 = new SubscriberEvent(streamIdA, startPositionA, businessEvent);
// 			var subscriberEventA2 = new SubscriberEvent(streamIdA, startPositionA + 1, businessEvent);
// 			var subscriberEventB1 = new SubscriberEvent(streamIdB, startPositionB, businessEvent);
// 			var subscriberEventB2 = new SubscriberEvent(streamIdB, startPositionB + 1, businessEvent);
// 			var parallelKeyA = "pkA";
// 			var parallelKeyB = "pkB";
// 			var parallelHold1 = new ManualResetEventSlim(false);
// 			var bShouldThrow = true;

// 			mockQueue.Setup(x => x.EnqueueWithWaitAsync(parallelKeyA, subscriberEventA1, cts.Token)).Returns(parallelHold1.WaitHandle.AsTask());
// 			mockQueue.Setup(x => x.EnqueueWithWaitAsync(parallelKeyB, subscriberEventB1, cts.Token)).Returns(parallelHold1.WaitHandle.AsTask());
// 			mockQueue.Setup(x => x.EnqueueWithWaitAsync(parallelKeyA, subscriberEventA2, cts.Token)).Callback(() => { if (bShouldThrow) throw new Exception(); }).Returns(Task.CompletedTask);
// 			mockQueue.Setup(x => x.EnqueueWithWaitAsync(parallelKeyB, subscriberEventB2, cts.Token)).Callback(() => { if (bShouldThrow) throw new Exception(); }).Returns(Task.CompletedTask);

// 			// Expecting these two to execute immediately.
// 			var taskA1 = manager.ReceiveSubscriberEventAsync(parallelKeyA, subscriberEventA1, cts.Token);
// 			var taskB1 = manager.ReceiveSubscriberEventAsync(parallelKeyB, subscriberEventB1, cts.Token);

// 			// Expecting these two to hold for the first two.
// 			// var taskA2 = manager.ReceiveSubscriberEventAsync(parallelKeyA, subscriberEventA1, cts.Token);
// 			// var taskB2 = manager.ReceiveSubscriberEventAsync(parallelKeyB, subscriberEventB2, cts.Token);

// 			await Task.WhenAny(new[] { Task.WhenAll(new[] { taskA1, taskB1 }), cts.Token.WaitHandle.AsTask() });
// 			if (cts.IsCancellationRequested) throw new TimeoutException();

// 			// Allow the first tasks to complete.
// 			// bShouldThrow = false;
// 			// parallelHold1.Set();

// 			// await Task.WhenAny(new[] { Task.WhenAll(new[] { taskA2, taskB2 }), cts.Token.WaitHandle.AsTask() });
// 			// if (cts.IsCancellationRequested) throw new TimeoutException();

// 			// mockQueue.Verify(x => x.EnqueueWithWaitAsync(parallelKeyA, subscriberEventA1, cts.Token));
// 			// mockQueue.Verify(x => x.EnqueueWithWaitAsync(parallelKeyA, subscriberEventB1, cts.Token));
// 			// mockQueue.Verify(x => x.EnqueueWithWaitAsync(parallelKeyB, subscriberEventA2, cts.Token));
// 			// mockQueue.Verify(x => x.EnqueueWithWaitAsync(parallelKeyB, subscriberEventB2, cts.Token));
// 		}

// 		[Fact]
// 		public async Task wait_for_enqueue_when_managing_and_no_events_in_queue()
// 		{
// 			var cts = new CancellationTokenSource(10000);
// 			var mockQueue = new Mock<IHandlingQueue>();
// 			var manager = new HandlingManager(NullStandardLogger.Instance, null, mockQueue.Object, null, 1);
// 			var enqueueTrigger = new ManualResetEventSlim(false);
// 			var manageIsWaitingSignal = new ManualResetEventSlim(false);
// 			var streamId = "s";
// 			var position = 1;
// 			var businessEvent = new BusinessEvent(BusinessMetadata.Empty);
// 			var subscriberEvent = new SubscriberEvent(streamId, position, businessEvent);
// 			var parallelKey = "x"; // This can be anything.

// 			mockQueue.Setup(x => x.TryDequeue(new string[] { })).Returns(new HandlingQueueItem(parallelKey, subscriberEvent));
// 			mockQueue.Setup(x => x.EnqueueTrigger).Callback(() => manageIsWaitingSignal.Set()).Returns(enqueueTrigger);

// 			var manageTask = manager.ManageAsync(cts.Token);

// 			await Task.WhenAny(new[] { manageIsWaitingSignal.WaitHandle.AsTask(), cts.Token.WaitHandle.AsTask() });
// 			if (cts.IsCancellationRequested) throw new TimeoutException();

// 			cts.Cancel();

// 			await Task.WhenAny(new[] { manageTask, new CancellationTokenSource(1000).Token.WaitHandle.AsTask() });

// 			Assert.True(manageTask.IsCompletedSuccessfully);
// 		}

// 		[Fact]
// 		public async Task receive_and_enqueue_subscriber_event()
// 		{
// 			var cts = new CancellationTokenSource(10000);
// 			var mockQueue = new Mock<IHandlingQueue>();
// 			var streamId = "s";
// 			var position = 1;
// 			var manager = new HandlingManager(NullStandardLogger.Instance, null, mockQueue.Object, null, 1);
// 			var businessEvent = new BusinessEvent(BusinessMetadata.Empty);
// 			var subscriberEvent = new SubscriberEvent(streamId, position, businessEvent);
// 			var parallelKey = "x";

// 			mockQueue.Setup(x => x.EnqueueWithWaitAsync(It.IsAny<string>(), It.IsAny<SubscriberEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

// 			await manager.ReceiveSubscriberEventAsync(parallelKey, subscriberEvent, cts.Token);

// 			mockQueue.Verify(x => x.EnqueueWithWaitAsync(parallelKey, subscriberEvent, cts.Token));
// 		}

// 		[Fact]
// 		public async Task run_handler_tasks_and_wait_for_throttle()
// 		{
// 			var cts = new CancellationTokenSource(10000);
// 			var mockHandler = new Mock<ISubscriberEventHandler>();
// 			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
// 			var streamId = "s";
// 			var position = 1;
// 			var maxParallelExcutions = 1; // Must be 1.
// 			var manager = new HandlingManager(NullStandardLogger.Instance, mockStreamStateRepo.Object, null, mockHandler.Object, maxParallelExcutions);
// 			var businessEvent = new BusinessEvent(BusinessMetadata.Empty);
// 			var subscriberEvent1 = new SubscriberEvent(streamId, position, businessEvent);
// 			var subscriberEvent2 = new SubscriberEvent(streamId, position, businessEvent);
// 			var streamState = new StreamState(position, false);
// 			var handlerHold1 = new ManualResetEventSlim(false);
// 			var parallelKey = "x"; // Shared for both calls.

// 			mockStreamStateRepo
// 				.Setup(x => x.LoadStreamStateAsync(streamId))
// 				.Returns(Task.FromResult<StreamState>(streamState));

// 			mockHandler.Setup(x => x.HandleAsync(subscriberEvent1, It.IsAny<CancellationToken>())).Returns(() => handlerHold1.WaitHandle.AsTask());
// 			mockHandler.Setup(x => x.HandleAsync(subscriberEvent2, It.IsAny<CancellationToken>())).Returns(() => Task.CompletedTask);

// 			Assert.Equal(1, manager.ThrottleCurrentCount);

// 			await manager.HandleSubscriberEventAsync(parallelKey, subscriberEvent1, cts.Token);
// 			if (cts.IsCancellationRequested) throw new TimeoutException();

// 			// Because both calls use the same parallel key we'll get an error if this second call
// 			// attempts to add a task with the same key.
// 			var runTask2 = manager.HandleSubscriberEventAsync(parallelKey, subscriberEvent2, cts.Token);

// 			Assert.Equal(0, manager.ThrottleCurrentCount);

// 			handlerHold1.Set();

// 			await Task.WhenAny(new[] { runTask2, cts.Token.WaitHandle.AsTask() });
// 			if (cts.IsCancellationRequested) throw new TimeoutException();

// 			mockHandler.Verify(x => x.HandleAsync(subscriberEvent1, cts.Token));
// 			mockHandler.Verify(x => x.HandleAsync(subscriberEvent2, cts.Token));
// 		}

// 		[Fact]
// 		public async Task not_run_handler_task_when_stream_state_is_errored()
// 		{
// 			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
// 			var streamId = "s";
// 			var position = 1;
// 			var manager = new HandlingManager(NullStandardLogger.Instance, mockStreamStateRepo.Object, null, null, 1);
// 			var businessEvent = new BusinessEvent(BusinessMetadata.Empty);
// 			var subscriberEvent = new SubscriberEvent(streamId, position, businessEvent);
// 			var parallelKey = "x";
// 			var streamState = new StreamState(position, true); // Has error.

// 			mockStreamStateRepo
// 				.Setup(x => x.LoadStreamStateAsync(streamId))
// 				.Returns(Task.FromResult<StreamState>(streamState));

// 			var result = await manager.HandleSubscriberEventAsync(parallelKey, subscriberEvent, CancellationToken.None);

// 			Assert.False(result);
// 		}

// 		[Fact]
// 		public async Task run_handler_and_save_stream_state_with_success()
// 		{
// 			var cts = new CancellationTokenSource();
// 			var mockHandler = new Mock<ISubscriberEventHandler>();
// 			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
// 			var streamId = "s";
// 			var position = 1;
// 			var manager = new HandlingManager(NullStandardLogger.Instance, mockStreamStateRepo.Object, null, mockHandler.Object, 1);
// 			var businessEvent = new BusinessEvent(BusinessMetadata.Empty);
// 			var subscriberEvent = new SubscriberEvent(streamId, position, businessEvent);
// 			var hasError = false; // Should not have error.

// 			mockHandler.Setup(x => x.HandleAsync(It.IsAny<SubscriberEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
// 			mockStreamStateRepo.Setup(x => x.SaveStreamStateAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

// 			await manager.RunHandlerAsync(subscriberEvent, cts.Token);

// 			mockHandler.Verify(x => x.HandleAsync(subscriberEvent, cts.Token));
// 			mockStreamStateRepo.Verify(x => x.SaveStreamStateAsync(streamId, position, hasError));
// 		}

// 		[Fact]
// 		public async Task run_handler_and_save_errored_stream_state()
// 		{
// 			var cts = new CancellationTokenSource();
// 			var mockHandler = new Mock<ISubscriberEventHandler>();
// 			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
// 			var streamId = "s";
// 			var position = 1;
// 			var manager = new HandlingManager(NullStandardLogger.Instance, mockStreamStateRepo.Object, null, mockHandler.Object, 1);
// 			var businessEvent = new BusinessEvent(BusinessMetadata.Empty);
// 			var subscriberEvent = new SubscriberEvent(streamId, position, businessEvent);
// 			var hasError = true; // Should have error after exception.

// 			mockHandler.Setup(x => x.HandleAsync(It.IsAny<SubscriberEvent>(), It.IsAny<CancellationToken>())).Throws(new Exception());
// 			mockStreamStateRepo.Setup(x => x.SaveStreamStateAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

// 			await manager.RunHandlerAsync(subscriberEvent, cts.Token);

// 			mockHandler.Verify(x => x.HandleAsync(subscriberEvent, cts.Token));
// 			mockStreamStateRepo.Verify(x => x.SaveStreamStateAsync(streamId, position, hasError));
// 		}
// 	}
// }
