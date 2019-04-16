using EventCore.EventSourcing;
using EventCore.Utilities;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventCore.EventSourcing.StatefulSubscriber.Tests
{
	public class ResolutionManagerTests
	{
		private class TestBusinessEvent : BusinessEvent
		{
			public TestBusinessEvent(BusinessEventMetadata metadata) : base(metadata) { }
		}

		private class TestException : Exception { }

		[Fact]
		public async Task manage_until_cancelled()
		{
			var cts = new CancellationTokenSource();
			var mockQueue = new Mock<IResolutionQueue>();
			var manager = new ResolutionManager(NullStandardLogger.Instance, null, null, mockQueue.Object, null);
			var awaitingEnqueueSignal = new ManualResetEventSlim(false);
			var mockEnqueueSignal = new ManualResetEventSlim(false);

			mockQueue.Setup(x => x.TryDequeue()).Returns((StreamEvent)null);
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
			var mockQueue = new Mock<IResolutionQueue>();
			var manager = new ResolutionManager(NullStandardLogger.Instance, null, null, mockQueue.Object, null);

			mockQueue.Setup(x => x.TryDequeue()).Throws(ex);

			await Assert.ThrowsAsync<TestException>(() => manager.ManageAsync(cts.Token));
		}

		[Fact]
		public async Task resolve_stream_event_without_link_and_send_to_sorting_manager()
		{
			var cts = new CancellationTokenSource(10000);
			var mockResolver = new Mock<IBusinessEventResolver>();
			var mockQueue = new Mock<IResolutionQueue>();
			var mockSortingManager = new Mock<ISortingManager>();
			var manager = new ResolutionManager(NullStandardLogger.Instance, mockResolver.Object, null, mockQueue.Object, mockSortingManager.Object);
			var streamId = "s";
			var position = 1;
			var eventType = "x";
			var data = new byte[] { };
			var streamEvent = new StreamEvent(streamId, position, null, eventType, data);
			var businessEvent = new TestBusinessEvent(BusinessEventMetadata.Empty);

			mockResolver.Setup(x => x.ResolveEvent(eventType, data)).Returns(businessEvent);
			mockQueue.Setup(x => x.TryDequeue()).Returns(streamEvent);
			mockSortingManager
				.Setup(x => x.ReceiveSubscriberEventAsync(It.IsAny<SubscriberEvent>(), It.IsAny<CancellationToken>()))
				.Callback(() => cts.Cancel())
				.Returns(Task.CompletedTask);

			await manager.ManageAsync(cts.Token);

			mockSortingManager.Verify(x => x.ReceiveSubscriberEventAsync(
				It.Is<SubscriberEvent>(e =>
					e.StreamId == streamId && e.Position == position
					&& e.SubscriptionStreamId == streamId && e.SubscriptionPosition == position
					&& e.ResolvedEvent == businessEvent
					),
					cts.Token
				));
		}

		[Fact]
		public async Task resolve_stream_event_with_link_and_send_to_sorting_manager()
		{
			var cts = new CancellationTokenSource(10000);
			var mockResolver = new Mock<IBusinessEventResolver>();
			var mockQueue = new Mock<IResolutionQueue>();
			var mockSortingManager = new Mock<ISortingManager>();
			var manager = new ResolutionManager(NullStandardLogger.Instance, mockResolver.Object, null, mockQueue.Object, mockSortingManager.Object);
			var streamId = "str";
			var position = 1;
			var subStreamId = "sub";
			var subPosition = 20;
			var eventType = "x";
			var data = new byte[] { };
			var streamEvent = new StreamEvent(subStreamId, subPosition, new StreamEventLink(streamId, position), eventType, data);
			var businessEvent = new TestBusinessEvent(BusinessEventMetadata.Empty);

			mockResolver.Setup(x => x.ResolveEvent(eventType, data)).Returns(businessEvent);
			mockQueue.Setup(x => x.TryDequeue()).Returns(streamEvent);
			mockSortingManager
				.Setup(x => x.ReceiveSubscriberEventAsync(It.IsAny<SubscriberEvent>(), It.IsAny<CancellationToken>()))
				.Callback(() => cts.Cancel())
				.Returns(Task.CompletedTask);

			await manager.ManageAsync(cts.Token);

			mockSortingManager.Verify(x => x.ReceiveSubscriberEventAsync(
				It.Is<SubscriberEvent>(e =>
					e.StreamId == streamId && e.Position == position
					&& e.SubscriptionStreamId == subStreamId && e.SubscriptionPosition == subPosition
					&& e.ResolvedEvent == businessEvent
					),
					cts.Token
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
			var streamId = "s";
			var position = 1;
			var eventType = "x";
			var data = new byte[] { };
			var streamEvent = new StreamEvent(streamId, position, null, eventType, data);
			var businessEvent = new TestBusinessEvent(BusinessEventMetadata.Empty);
			var awaitingEnqueueSignal = new ManualResetEventSlim(false);
			var mockEnqueueSignal = new ManualResetEventSlim(false);

			mockResolver.Setup(x => x.ResolveEvent(eventType, data)).Returns(businessEvent);
			mockQueue.Setup(x => x.TryDequeue()).Returns(streamEvent);
			mockQueue.Setup(x => x.AwaitEnqueueSignalAsync()).Callback(() => awaitingEnqueueSignal.Set()).Returns(mockEnqueueSignal.WaitHandle.AsTask());

			var manageTask = manager.ManageAsync(cts.Token);

			await Task.WhenAny(new[] { awaitingEnqueueSignal.WaitHandle.AsTask(), cts.Token.WaitHandle.AsTask() });
			if (cts.IsCancellationRequested) throw new TimeoutException();

			cts.Cancel();
		}

		[Fact]
		public async Task receive_and_enqueue_stream_event_when_eligible()
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

		[Fact]
		public async Task receive_and_not_enqueue_stream_event_when_not_eligible()
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
			var streamState = new StreamState(lastAttemptedPosition, true);
			var manager = new ResolutionManager(NullStandardLogger.Instance, mockResolver.Object, mockStreamStateRepo.Object, mockQueue.Object, null);
			var streamEvent = new StreamEvent(streamId, newPosition, null, eventType, new byte[] { });

			mockStreamStateRepo.Setup(x => x.LoadStreamStateAsync(streamId)).ReturnsAsync(streamState);
			mockResolver.Setup(x => x.CanResolve(eventType)).Returns(false); // Make this event ineligible.

			await manager.ReceiveStreamEventAsync(streamEvent, firstPositionInStream, cts.Token);
			if (cts.IsCancellationRequested) throw new TimeoutException();

			mockQueue.VerifyNoOtherCalls();
		}

		[Fact]
		public async Task stream_eligible_for_resolution_when_stream_state_exists()
		{
			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
			var mockResolver = new Mock<IBusinessEventResolver>();
			var streamId = "s";
			var newPosition = 1;
			var eventType = "x";
			var lastAttemptedPosition = 0;
			var firstPositionInStream = 0;
			var streamState = new StreamState(lastAttemptedPosition, false);
			var manager = new ResolutionManager(NullStandardLogger.Instance, mockResolver.Object, mockStreamStateRepo.Object, null, null);
			var streamEvent = new StreamEvent(streamId, newPosition, null, eventType, new byte[] { });

			mockStreamStateRepo.Setup(x => x.LoadStreamStateAsync(streamId)).ReturnsAsync(streamState);
			mockResolver.Setup(x => x.CanResolve(eventType)).Returns(true);

			var eligibility = await manager.IsStreamEventEligibleForResolution(streamEvent, firstPositionInStream);

			Assert.Equal(ResolutionEligibility.Eligible, eligibility);
		}

		[Fact]
		public async Task stream_eligible_for_resolution_when_stream_state_not_exists()
		{
			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
			var streamId = "s";
			var newPosition = 0; // Must be same as first position for this test.
			var eventType = "x";
			var firstPositionInStream = 0;
			var manager = new ResolutionManager(NullStandardLogger.Instance, null, mockStreamStateRepo.Object, null, null);
			var streamEvent = new StreamEvent(streamId, newPosition, null, eventType, new byte[] { });

			mockStreamStateRepo.Setup(x => x.LoadStreamStateAsync(streamId)).ReturnsAsync((StreamState)null);

			var eligibility = await manager.IsStreamEventEligibleForResolution(streamEvent, firstPositionInStream);

			Assert.Equal(ResolutionEligibility.Eligible, eligibility);
		}

		[Fact]
		public async Task stream_ineligible_for_resolution_skipped_already_processed()
		{
			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
			var streamId = "s";
			var newPosition = 1;
			var eventType = "x";
			var lastAttemptedPosition = newPosition + 1; // Must be greater than new position for this test.
			var firstPositionInStream = 0;
			var streamState = new StreamState(lastAttemptedPosition, false);
			var manager = new ResolutionManager(NullStandardLogger.Instance, null, mockStreamStateRepo.Object, null, null);
			var streamEvent = new StreamEvent(streamId, newPosition, null, eventType, new byte[] { });

			mockStreamStateRepo.Setup(x => x.LoadStreamStateAsync(streamId)).ReturnsAsync(streamState);

			var eligibility = await manager.IsStreamEventEligibleForResolution(streamEvent, firstPositionInStream);

			Assert.Equal(ResolutionEligibility.SkippedAlreadyProcessed, eligibility);
		}

		[Fact]
		public async Task stream_ineligible_for_resolution_unable_has_stream_error()
		{
			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
			var streamId = "s";
			var newPosition = 1;
			var eventType = "x";
			var lastAttemptedPosition = 0;
			var firstPositionInStream = 0;
			var streamState = new StreamState(lastAttemptedPosition, true);
			var manager = new ResolutionManager(NullStandardLogger.Instance, null, mockStreamStateRepo.Object, null, null);
			var streamEvent = new StreamEvent(streamId, newPosition, null, eventType, new byte[] { });

			mockStreamStateRepo.Setup(x => x.LoadStreamStateAsync(streamId)).ReturnsAsync(streamState);

			var eligibility = await manager.IsStreamEventEligibleForResolution(streamEvent, firstPositionInStream);

			Assert.Equal(ResolutionEligibility.UnableStreamHasError, eligibility);
		}

		[Fact]
		public async Task stream_ineligible_for_resolution_unable_to_resolve_event_type()
		{
			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
			var mockResolver = new Mock<IBusinessEventResolver>();
			var streamId = "s";
			var newPosition = 1;
			var eventType = "x";
			var lastAttemptedPosition = 0;
			var firstPositionInStream = 0;
			var streamState = new StreamState(lastAttemptedPosition, false);
			var manager = new ResolutionManager(NullStandardLogger.Instance, mockResolver.Object, mockStreamStateRepo.Object, null, null);
			var streamEvent = new StreamEvent(streamId, newPosition, null, eventType, new byte[] { });

			mockStreamStateRepo.Setup(x => x.LoadStreamStateAsync(streamId)).ReturnsAsync(streamState);
			mockResolver.Setup(x => x.CanResolve(eventType)).Returns(false);

			var eligibility = await manager.IsStreamEventEligibleForResolution(streamEvent, firstPositionInStream);

			Assert.Equal(ResolutionEligibility.UnableToResolveEventType, eligibility);
		}

		[Fact]
		public async Task stream_eligible_for_resolution_throws_non_sequential_position_and_stream_state_not_exists()
		{
			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
			var streamId = "s";
			var newPosition = 1; // Must be first position in stream + 1 for this test.
			var eventType = "x";
			var firstPositionInStream = 0;
			var manager = new ResolutionManager(NullStandardLogger.Instance, null, mockStreamStateRepo.Object, null, null);
			var streamEvent = new StreamEvent(streamId, newPosition, null, eventType, new byte[] { });

			mockStreamStateRepo.Setup(x => x.LoadStreamStateAsync(streamId)).ReturnsAsync((StreamState)null);

			await Assert.ThrowsAsync<InvalidOperationException>(() => manager.IsStreamEventEligibleForResolution(streamEvent, firstPositionInStream));
		}

		[Fact]
		public async Task stream_eligible_for_resolution_throws_non_sequential_position_and_stream_state_exists()
		{
			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
			var mockResolver = new Mock<IBusinessEventResolver>();
			var streamId = "s";
			var newPosition = 2; // Must be greater than last attempted position + 1 for this test.
			var eventType = "x";
			var lastAttemptedPosition = 0;
			var firstPositionInStream = 0;
			var streamState = new StreamState(lastAttemptedPosition, false);
			var manager = new ResolutionManager(NullStandardLogger.Instance, mockResolver.Object, mockStreamStateRepo.Object, null, null);
			var streamEvent = new StreamEvent(streamId, newPosition, null, eventType, new byte[] { });

			mockStreamStateRepo.Setup(x => x.LoadStreamStateAsync(streamId)).ReturnsAsync(streamState);
			mockResolver.Setup(x => x.CanResolve(eventType)).Returns(true);

			await Assert.ThrowsAsync<InvalidOperationException>(() => manager.IsStreamEventEligibleForResolution(streamEvent, firstPositionInStream));
		}
	}
}
