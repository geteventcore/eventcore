using System;
using System.Threading;
using System.Threading.Tasks;
using EventCore.EventSourcing;
using EventCore.Utilities;
using Moq;
using Xunit;

namespace EventCore.StatefulSubscriber.Tests
{
	public class HandlingManagerHandlerRunnerTests
	{
		[Fact]
		public async Task run_handler_and_save_stream_state_with_success()
		{
			var cts = new CancellationTokenSource();
			var mockAwaiter = new Mock<IHandlingManagerAwaiter>();
			var mockHandler = new Mock<ISubscriberEventHandler>();
			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
			var runner = new HandlingManagerHandlerRunner(NullStandardLogger.Instance, mockAwaiter.Object, mockStreamStateRepo.Object, mockHandler.Object);
			var streamId = "s";
			var position = 1;
			var subscriberEvent = new SubscriberEvent(null, streamId, position, null, null);

			mockHandler.Setup(x => x.HandleSubscriberEventAsync(It.IsAny<SubscriberEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
			mockStreamStateRepo.Setup(x => x.SaveStreamStateAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

			await runner.TryRunHandlerAsync(subscriberEvent, cts.Token);

			mockHandler.Verify(x => x.HandleSubscriberEventAsync(subscriberEvent, cts.Token));
			mockStreamStateRepo.Verify(x => x.SaveStreamStateAsync(streamId, position, false));
		}

		[Fact]
		public async Task run_handler_and_save_errored_stream_state()
		{
			var cts = new CancellationTokenSource();
			var mockAwaiter = new Mock<IHandlingManagerAwaiter>();
			var mockHandler = new Mock<ISubscriberEventHandler>();
			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
			var runner = new HandlingManagerHandlerRunner(NullStandardLogger.Instance, mockAwaiter.Object, mockStreamStateRepo.Object, mockHandler.Object);
			var streamId = "s";
			var position = 1;
			var subscriberEvent = new SubscriberEvent(null, streamId, position, null, null);

			mockHandler.Setup(x => x.HandleSubscriberEventAsync(It.IsAny<SubscriberEvent>(), It.IsAny<CancellationToken>())).Throws(new Exception());
			mockStreamStateRepo.Setup(x => x.SaveStreamStateAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

			await runner.TryRunHandlerAsync(subscriberEvent, cts.Token);

			mockHandler.Verify(x => x.HandleSubscriberEventAsync(subscriberEvent, cts.Token));
			mockStreamStateRepo.Verify(x => x.SaveStreamStateAsync(streamId, position, true));
		}

		[Fact]
		public async Task reset_awaiter_when_success()
		{
			var mockAwaiter = new Mock<IHandlingManagerAwaiter>();
			var mockHandler = new Mock<ISubscriberEventHandler>();
			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
			var runner = new HandlingManagerHandlerRunner(NullStandardLogger.Instance, mockAwaiter.Object, mockStreamStateRepo.Object, mockHandler.Object);
			var subscriberEvent = new SubscriberEvent(null, null, 0, null, null);

			mockHandler.Setup(x => x.HandleSubscriberEventAsync(It.IsAny<SubscriberEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
			mockStreamStateRepo.Setup(x => x.SaveStreamStateAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

			await runner.TryRunHandlerAsync(subscriberEvent, CancellationToken.None);

			mockAwaiter.Verify(x => x.ReleaseThrottle());
			mockAwaiter.Verify(x => x.SetHandlerCompletionSignal());
		}

		[Fact]
		public async Task reset_awaiter_when_error()
		{
			var cts = new CancellationTokenSource();
			var mockAwaiter = new Mock<IHandlingManagerAwaiter>();
			var mockHandler = new Mock<ISubscriberEventHandler>();
			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
			var runner = new HandlingManagerHandlerRunner(NullStandardLogger.Instance, mockAwaiter.Object, mockStreamStateRepo.Object, mockHandler.Object);
			var subscriberEvent = new SubscriberEvent(null, null, 0, null, null);

			mockHandler.Setup(x => x.HandleSubscriberEventAsync(It.IsAny<SubscriberEvent>(), It.IsAny<CancellationToken>())).Throws(new Exception());
			mockStreamStateRepo.Setup(x => x.SaveStreamStateAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

			await runner.TryRunHandlerAsync(subscriberEvent, CancellationToken.None);

			mockAwaiter.Verify(x => x.ReleaseThrottle());
			mockAwaiter.Verify(x => x.SetHandlerCompletionSignal());
		}
	}
}
