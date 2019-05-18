using EventCore.EventSourcing;
using EventCore.Utilities;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventCore.StatefulSubscriber.Tests
{
	public class SubscriptionListenerTests
	{
		private class TestException : Exception { }

		[Fact]
		public async Task listen_with_correct_params_until_cancelled()
		{
			var cts = new CancellationTokenSource();
			var mockStreamClientFactory = new Mock<IStreamClientFactory>();
			var mockStreamClient = new Mock<IStreamClient>();
			var listener = new SubscriptionListener(NullStandardLogger.Instance, mockStreamClientFactory.Object, null);
			var regionId = "x";
			var subscriptionStreamId = "s";
			var firstPositionInStream = 1;
			var awaitingEnqueueSignal = new ManualResetEventSlim(false);
			var mockEnqueueSignal = new ManualResetEventSlim(false);

			mockStreamClientFactory.Setup(x => x.Create(regionId)).Returns(mockStreamClient.Object);
			mockStreamClient.Setup(x => x.FirstPositionInStream).Returns(firstPositionInStream);
			mockStreamClient
				.Setup(x => x.SubscribeToStreamAsync(subscriptionStreamId, firstPositionInStream, It.IsAny<Func<StreamEvent, Task>>(), cts.Token))
				.Callback(() => awaitingEnqueueSignal.Set())
				.Returns(mockEnqueueSignal.WaitHandle.AsTask());

			var listenTask = listener.ListenAsync(regionId, subscriptionStreamId, cts.Token);

			var timeoutToken1 = new CancellationTokenSource(10000).Token;
			await Task.WhenAny(new[] { awaitingEnqueueSignal.WaitHandle.AsTask(), timeoutToken1.WaitHandle.AsTask() });
			if (timeoutToken1.IsCancellationRequested) throw new TimeoutException();

			cts.Cancel();

			var timeoutToken2 = new CancellationTokenSource(10000).Token;
			await Task.WhenAny(new[] { listenTask, timeoutToken2.WaitHandle.AsTask() });
			if (timeoutToken2.IsCancellationRequested) throw new TimeoutException();
		}

		[Fact]
		public async Task rethrow_exception_when_listening()
		{
			var cts = new CancellationTokenSource(10000);
			var ex = new TestException();
			var mockStreamClientFactory = new Mock<IStreamClientFactory>();
			var mockStreamClient = new Mock<IStreamClient>();
			var listener = new SubscriptionListener(NullStandardLogger.Instance, mockStreamClientFactory.Object, null);
			var regionId = "x";
			var subscriptionStreamId = "s";
			var firstPositionInStream = 1;

			mockStreamClientFactory.Setup(x => x.Create(regionId)).Returns(mockStreamClient.Object);
			mockStreamClient.Setup(x => x.FirstPositionInStream).Returns(firstPositionInStream);
			mockStreamClient
				.Setup(x => x.SubscribeToStreamAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<Func<StreamEvent, Task>>(), It.IsAny<CancellationToken>()))
				.Throws(new TestException());

			await Assert.ThrowsAsync<TestException>(() => listener.ListenAsync(regionId, subscriptionStreamId, cts.Token));
		}
	}
}
