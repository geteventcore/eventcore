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
		public async Task resolve_stream_event_and_send_to_sorting_manager()
		{
			var cts = new CancellationTokenSource(10000);
			var mockResolver = new Mock<IBusinessEventResolver>();
			var mockQueue = new Mock<IResolutionQueue>();
			var mockSortingManager = new Mock<ISortingManager>();
			var manager = new ResolutionManager(NullStandardLogger.Instance, mockResolver.Object, null, mockQueue.Object, mockSortingManager.Object);
			var enqueueTrigger = new ManualResetEventSlim(false);
			var streamId = "s";
			var position = 1;
			var eventType = "x";
			var data = new byte[] { };
			var streamEvent = new StreamEvent(streamId, position, null, eventType, data);
			var businessEvent = new BusinessEvent(BusinessMetadata.Empty);

			mockResolver.Setup(x => x.ResolveEvent(eventType, data)).Returns(businessEvent);
			mockQueue.Setup(x => x.TryDequeue()).Returns(streamEvent);
			mockQueue.Setup(x => x.EnqueueTrigger).Returns(enqueueTrigger);
			mockSortingManager
				.Setup(x => x.ReceiveSubscriberEventAsync(It.IsAny<SubscriberEvent>(), It.IsAny<CancellationToken>()))
				.Callback(() => cts.Cancel())
				.Returns(Task.CompletedTask);

			await manager.ManageAsync(cts.Token);

			mockSortingManager.Verify(x => x.ReceiveSubscriberEventAsync(
				It.Is<SubscriberEvent>(e => e.StreamId == streamId && e.Position == position && e.ResolvedEvent == businessEvent), cts.Token
				));
		}

		[Fact]
		public async Task wait_for_enqueue_when_managing_and_no_events_in_queue()
		{
			var cts = new CancellationTokenSource(10000);
			var mockResolver = new Mock<IBusinessEventResolver>();
			var mockQueue = new Mock<IResolutionQueue>();
			var mockSortingManager = new Mock<ISortingManager>();
			var manager = new ResolutionManager(NullStandardLogger.Instance, mockResolver.Object, null, mockQueue.Object, mockSortingManager.Object);
			var enqueueTrigger = new ManualResetEventSlim(false);
			var manageIsWaitingSignal = new ManualResetEventSlim(false);
			var streamId = "s";
			var position = 1;
			var eventType = "x";
			var data = new byte[] { };
			var streamEvent = new StreamEvent(streamId, position, null, eventType, data);
			var businessEvent = new BusinessEvent(BusinessMetadata.Empty);

			mockResolver.Setup(x => x.ResolveEvent(eventType, data)).Returns(businessEvent);
			mockQueue.Setup(x => x.TryDequeue()).Returns(streamEvent);
			mockQueue.Setup(x => x.EnqueueTrigger).Callback(() => manageIsWaitingSignal.Set()).Returns(enqueueTrigger);

			var manageTask = manager.ManageAsync(cts.Token);

			await Task.WhenAny(new[] { manageIsWaitingSignal.WaitHandle.AsTask(), cts.Token.WaitHandle.AsTask() });
			if (cts.IsCancellationRequested) throw new TimeoutException();

			cts.Cancel();

			await Task.WhenAny(new[] { manageTask, new CancellationTokenSource(1000).Token.WaitHandle.AsTask() });
			
			Assert.True(manageTask.IsCompletedSuccessfully);
		}

		[Fact]
		public async Task enqueue_subscriber_event()
		{
			var cts = new CancellationTokenSource(10000);
			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
			var mockResolver = new Mock<IBusinessEventResolver>();
			var mockQueue = new Mock<IResolutionQueue>();
			var streamId = "s";
			var newPosition = 1;
			var eventType = "x";
			var lastAttemptedPosition = 0;
			var firstPositionInStream = 0;
			var streamState = new StreamState(lastAttemptedPosition, false);
			var manager = new ResolutionManager(NullStandardLogger.Instance, mockResolver.Object, mockStreamStateRepo.Object, mockQueue.Object, null);
			var streamEvent = new StreamEvent(streamId, newPosition, null, eventType, new byte[] { });

			mockStreamStateRepo.Setup(x => x.LoadStreamStateAsync(streamId)).ReturnsAsync(streamState);
			mockResolver.Setup(x => x.CanResolve(eventType)).Returns(true);

			await manager.ReceiveStreamEventAsync(streamEvent, firstPositionInStream, cts.Token);
			if (cts.IsCancellationRequested) throw new TimeoutException();

			mockQueue.Verify(x => x.EnqueueWithWaitAsync(streamEvent, cts.Token));
		}
	}
}
