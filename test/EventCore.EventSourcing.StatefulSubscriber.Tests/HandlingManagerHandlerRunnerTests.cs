using System;
using System.Threading;
using System.Threading.Tasks;
using EventCore.EventSourcing;
using EventCore.Utilities;
using Moq;
using Xunit;

namespace EventCore.EventSourcing.StatefulSubscriber.Tests
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
			var businessEvent = new BusinessEvent(BusinessMetadata.Empty);
			var subscriberEvent = new SubscriberEvent(streamId, position, businessEvent);

			mockHandler.Setup(x => x.HandleAsync(It.IsAny<SubscriberEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
			mockStreamStateRepo.Setup(x => x.SaveStreamStateAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

			await runner.TryRunHandlerAsync(subscriberEvent, cts.Token);

			mockHandler.Verify(x => x.HandleAsync(subscriberEvent, cts.Token));
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
			var businessEvent = new BusinessEvent(BusinessMetadata.Empty);
			var subscriberEvent = new SubscriberEvent(streamId, position, businessEvent);

			mockHandler.Setup(x => x.HandleAsync(It.IsAny<SubscriberEvent>(), It.IsAny<CancellationToken>())).Throws(new Exception());
			mockStreamStateRepo.Setup(x => x.SaveStreamStateAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

			await runner.TryRunHandlerAsync(subscriberEvent, cts.Token);

			mockHandler.Verify(x => x.HandleAsync(subscriberEvent, cts.Token));
			mockStreamStateRepo.Verify(x => x.SaveStreamStateAsync(streamId, position, true));
		}

		[Fact]
		public async Task reset_awaiter_when_success()
		{
			var mockAwaiter = new Mock<IHandlingManagerAwaiter>();
			var mockHandler = new Mock<ISubscriberEventHandler>();
			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
			var runner = new HandlingManagerHandlerRunner(NullStandardLogger.Instance, mockAwaiter.Object, mockStreamStateRepo.Object, mockHandler.Object);
			var streamId = "s";
			var position = 1;
			var businessEvent = new BusinessEvent(BusinessMetadata.Empty);
			var subscriberEvent = new SubscriberEvent(streamId, position, businessEvent);

			mockHandler.Setup(x => x.HandleAsync(It.IsAny<SubscriberEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
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
			var streamId = "s";
			var position = 1;
			var businessEvent = new BusinessEvent(BusinessMetadata.Empty);
			var subscriberEvent = new SubscriberEvent(streamId, position, businessEvent);

			mockHandler.Setup(x => x.HandleAsync(It.IsAny<SubscriberEvent>(), It.IsAny<CancellationToken>())).Throws(new Exception());
			mockStreamStateRepo.Setup(x => x.SaveStreamStateAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

			await runner.TryRunHandlerAsync(subscriberEvent, CancellationToken.None);

			mockAwaiter.Verify(x => x.ReleaseThrottle());
			mockAwaiter.Verify(x => x.SetHandlerCompletionSignal());
		}
	}
}
