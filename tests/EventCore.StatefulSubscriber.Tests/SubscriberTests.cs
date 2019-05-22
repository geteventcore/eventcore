using EventCore.EventSourcing;
using EventCore.Utilities;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventCore.StatefulSubscriber.Tests
{
	public class SubscriberTests
	{
		private class TestException : Exception { }

		[Fact]
		public async Task listen_and_start_managers_until_cancelled()
		{
			var cts = new CancellationTokenSource();
			var mockSubscriptionListener = new Mock<ISubscriptionListener>();
			var mockResolutionManager = new Mock<IResolutionManager>();
			var mockSortingManager = new Mock<ISortingManager>();
			var mockHandlingManager = new Mock<IHandlingManager>();
			var mockStreamClient = new Mock<IStreamClient>();
			var regionId = "r";
			var streamId = "s";
			var options = new SubscriberOptions(new List<SubscriptionStreamId>() { new SubscriptionStreamId(regionId, streamId) });

			var mockAwaiter = new ManualResetEventSlim(false);
			var listeningSignal = new ManualResetEventSlim(false);
			var resolutionManagingSignal = new ManualResetEventSlim(false);
			var sortingManagingSignal = new ManualResetEventSlim(false);
			var handlingManagingSignal = new ManualResetEventSlim(false);

			mockSubscriptionListener.Setup(x => x.ListenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Callback(() => listeningSignal.Set()).Returns(mockAwaiter.WaitHandle.AsTask());
			mockResolutionManager.Setup(x => x.ManageAsync(It.IsAny<CancellationToken>())).Callback(() => resolutionManagingSignal.Set()).Returns(mockAwaiter.WaitHandle.AsTask());
			mockSortingManager.Setup(x => x.ManageAsync(It.IsAny<CancellationToken>())).Callback(() => sortingManagingSignal.Set()).Returns(mockAwaiter.WaitHandle.AsTask());
			mockHandlingManager.Setup(x => x.ManageAsync(It.IsAny<CancellationToken>())).Callback(() => handlingManagingSignal.Set()).Returns(mockAwaiter.WaitHandle.AsTask());

			var subscriber = new Subscriber(NullStandardLogger.Instance, null, mockSubscriptionListener.Object, mockResolutionManager.Object, mockSortingManager.Object, mockHandlingManager.Object, null, options);

			var subscribeTask = subscriber.SubscribeAsync(cts.Token);

			var timeoutToken1 = new CancellationTokenSource(10000).Token;
			await Task.WhenAny(new[] { Task.WhenAll(new[] { listeningSignal.WaitHandle.AsTask(), resolutionManagingSignal.WaitHandle.AsTask(), sortingManagingSignal.WaitHandle.AsTask(), handlingManagingSignal.WaitHandle.AsTask() }), timeoutToken1.WaitHandle.AsTask() });
			if (timeoutToken1.IsCancellationRequested) throw new TimeoutException();

			mockSubscriptionListener.Verify(x => x.ListenAsync(regionId, streamId, cts.Token));
			mockResolutionManager.Verify(x => x.ManageAsync(cts.Token));
			mockSortingManager.Verify(x => x.ManageAsync(cts.Token));
			mockHandlingManager.Verify(x => x.ManageAsync(cts.Token));

			cts.Cancel();

			var timeoutToken2 = new CancellationTokenSource(10000).Token;
			await Task.WhenAny(new[] { subscribeTask, timeoutToken2.WaitHandle.AsTask() });
			if (timeoutToken2.IsCancellationRequested) throw new TimeoutException();
		}

		[Fact]
		public async Task subscribe_to_multiple_regions()
		{
			var cts = new CancellationTokenSource(10000);
			var mockSubscriptionListener = new Mock<ISubscriptionListener>();
			var mockResolutionManager = new Mock<IResolutionManager>();
			var mockSortingManager = new Mock<ISortingManager>();
			var mockHandlingManager = new Mock<IHandlingManager>();
			var mockStreamClient = new Mock<IStreamClient>();
			var regionId1 = "r1";
			var regionId2 = "r2";
			var streamId = "s";
			var options = new SubscriberOptions(new List<SubscriptionStreamId>() {
				new SubscriptionStreamId(regionId1, streamId),
				new SubscriptionStreamId(regionId2, streamId)
				});
			var subscriber = new Subscriber(NullStandardLogger.Instance, null, mockSubscriptionListener.Object, mockResolutionManager.Object, mockSortingManager.Object, mockHandlingManager.Object, null, options);
			var mockAwaiter = new ManualResetEventSlim(false);
			var listeningSignal = new ManualResetEventSlim(false);
			var resolutionManagingSignal = new ManualResetEventSlim(false);
			var sortingManagingSignal = new ManualResetEventSlim(false);
			var handlingManagingSignal = new ManualResetEventSlim(false);

			mockSubscriptionListener.Setup(x => x.ListenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
			mockResolutionManager.Setup(x => x.ManageAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
			mockSortingManager.Setup(x => x.ManageAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
			mockHandlingManager.Setup(x => x.ManageAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

			await subscriber.SubscribeAsync(cts.Token);
			if (cts.IsCancellationRequested) throw new TimeoutException();

			mockSubscriptionListener.Verify(x => x.ListenAsync(regionId1, streamId, cts.Token));
			mockSubscriptionListener.Verify(x => x.ListenAsync(regionId2, streamId, cts.Token));
		}

		[Fact]
		public async Task throw_when_subscribe_and_already_subscribing()
		{
			var cts = new CancellationTokenSource(10000);
			var mockSubscriptionListener = new Mock<ISubscriptionListener>();
			var mockResolutionManager = new Mock<IResolutionManager>();
			var mockSortingManager = new Mock<ISortingManager>();
			var mockHandlingManager = new Mock<IHandlingManager>();
			var mockStreamClient = new Mock<IStreamClient>();
			var regionId1 = "r1";
			var regionId2 = "r2";
			var streamId = "s";
			var options = new SubscriberOptions(new List<SubscriptionStreamId>() {
				new SubscriptionStreamId(regionId1, streamId),
				new SubscriptionStreamId(regionId2, streamId)
				});
			var subscriber = new Subscriber(NullStandardLogger.Instance, null, mockSubscriptionListener.Object, mockResolutionManager.Object, mockSortingManager.Object, mockHandlingManager.Object, null, options);
			var mockAwaiter = new ManualResetEventSlim(false);

			mockSubscriptionListener.Setup(x => x.ListenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(mockAwaiter.WaitHandle.AsTask());
			mockResolutionManager.Setup(x => x.ManageAsync(It.IsAny<CancellationToken>())).Returns(mockAwaiter.WaitHandle.AsTask());
			mockSortingManager.Setup(x => x.ManageAsync(It.IsAny<CancellationToken>())).Returns(mockAwaiter.WaitHandle.AsTask());
			mockHandlingManager.Setup(x => x.ManageAsync(It.IsAny<CancellationToken>())).Returns(mockAwaiter.WaitHandle.AsTask());

			var subscribeTask = subscriber.SubscribeAsync(cts.Token);

			await Assert.ThrowsAsync<InvalidOperationException>(() => subscriber.SubscribeAsync(cts.Token));

			cts.Cancel();
			await subscribeTask;
		}

		[Fact]
		public async Task throw_exception_when_subscribing()
		{
			var cts = new CancellationTokenSource(10000);
			var mockSubscriptionListener = new Mock<ISubscriptionListener>();
			var mockResolutionManager = new Mock<IResolutionManager>();
			var mockSortingManager = new Mock<ISortingManager>();
			var mockHandlingManager = new Mock<IHandlingManager>();
			var mockStreamClient = new Mock<IStreamClient>();
			var options = new SubscriberOptions(new List<SubscriptionStreamId>());
			var subscriber = new Subscriber(NullStandardLogger.Instance, null, mockSubscriptionListener.Object, mockResolutionManager.Object, mockSortingManager.Object, mockHandlingManager.Object, null, options);

			mockResolutionManager.Setup(x => x.ManageAsync(It.IsAny<CancellationToken>())).Throws(new TestException());
			mockSortingManager.Setup(x => x.ManageAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
			mockHandlingManager.Setup(x => x.ManageAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

			await Assert.ThrowsAsync<TestException>(() => subscriber.SubscribeAsync(cts.Token));
		}

		[Fact]
		public async Task reset_stream_states()
		{
			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
			var options = new SubscriberOptions(new List<SubscriptionStreamId>());
			var subscriber = new Subscriber(NullStandardLogger.Instance, null, null, null, null, null, mockStreamStateRepo.Object, options);

			await subscriber.ResetStreamStatesAsync();

			mockStreamStateRepo.Verify(x => x.ResetStreamStatesAsync());
		}

		[Fact]
		public async Task clear_stream_state_errors()
		{
			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
			var options = new SubscriberOptions(new List<SubscriptionStreamId>());
			var subscriber = new Subscriber(NullStandardLogger.Instance, null, null, null, null, null, mockStreamStateRepo.Object, options);

			await subscriber.ClearStreamStateErrorsAsync();

			mockStreamStateRepo.Verify(x => x.ClearStreamStateErrorsAsync());
		}

		[Fact]
		public async Task missing_test_for_end_of_subscriptions()
		{
			await Task.Delay(100);
			throw new NotImplementedException();
		}
	}
}
