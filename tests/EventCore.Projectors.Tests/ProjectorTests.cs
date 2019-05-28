using EventCore.EventSourcing;
using EventCore.StatefulSubscriber;
using EventCore.Utilities;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventCore.Projectors.Tests
{
	public class ProjectorTests
	{
		public class TestEvent : BusinessEvent
		{
			public TestEvent() : base(BusinessEventMetadata.Empty) { }
		}

		public class TestProjector : Projector,
			IHandleBusinessEvent<TestEvent>
		{
			public Func<string, long, IBusinessEvent, CancellationToken, Task> MockBusinessEventHandler;

			public TestProjector(ProjectorDependencies dependencies) : base(dependencies)
			{
			}

			public Task HandleBusinessEventAsync(string streamId, long position, TestEvent e, CancellationToken cancellationToken)
				=> MockBusinessEventHandler(streamId, position, e, cancellationToken);
		}

		[Fact]
		public void create_subscriber_when_construct()
		{
			var mockLogger = new Mock<IStandardLogger>();
			var mockSubFactory = new Mock<ISubscriberFactory>();
			var subFactoryOptions = new SubscriberFactoryOptions(0, 0, 0, 0);
			var mockStreamClientFactory = new Mock<IStreamClientFactory>();
			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
			var mockEventResolver = new Mock<IBusinessEventResolver>();
			var streamIds = new List<SubscriptionStreamId>();

			var dependencies = new ProjectorDependencies(
				mockLogger.Object, mockSubFactory.Object, subFactoryOptions, mockStreamClientFactory.Object, mockStreamStateRepo.Object,
				mockEventResolver.Object, streamIds
				);

			var projector = new TestProjector(dependencies);

			mockSubFactory.Verify(x => x.Create(
				mockLogger.Object, mockStreamClientFactory.Object, mockStreamStateRepo.Object,
				mockEventResolver.Object, projector, projector, subFactoryOptions, streamIds
			));
		}

		[Fact]
		public async Task subscribe_when_run()
		{
			var mockSubFactory = new Mock<ISubscriberFactory>();
			var mockSubscriber = new Mock<ISubscriber>();

			mockSubFactory.Setup(x => x.Create(
				It.IsAny<IStandardLogger>(), It.IsAny<IStreamClientFactory>(), It.IsAny<IStreamStateRepo>(),
				It.IsAny<IBusinessEventResolver>(), It.IsAny<ISubscriberEventSorter>(), It.IsAny<ISubscriberEventHandler>(),
				It.IsAny<SubscriberFactoryOptions>(), It.IsAny<IList<SubscriptionStreamId>>()
			)).Returns(mockSubscriber.Object);

			var dependencies = new ProjectorDependencies(null, mockSubFactory.Object, null, null, null, null, null);
			var projector = new TestProjector(dependencies);

			var tokenSource = new CancellationTokenSource(5000);

			await projector.RunAsync(tokenSource.Token);
			Assert.False(tokenSource.IsCancellationRequested);

			mockSubscriber.Verify(x => x.SubscribeAsync(tokenSource.Token));
		}

		[Fact]
		public async Task invoke_business_event_handler()
		{
			var mockSubFactory = new Mock<ISubscriberFactory>();
			var businessEvent = new TestEvent();
			var streamId = "s";
			var position = 5;
			var subEvent = new SubscriberEvent(null, streamId, position, null, businessEvent);
			var dependencies = new ProjectorDependencies(null, mockSubFactory.Object, null, null, null, null, null);
			var projector = new TestProjector(dependencies);
			var token = CancellationToken.None;
			var called = false;

			projector.MockBusinessEventHandler = (pStreamId, pPosition, pEvent, pToken) =>
			{
				Assert.Equal(streamId, pStreamId);
				Assert.Equal(position, pPosition);
				Assert.Equal(businessEvent, pEvent);
				Assert.Equal(token, pToken);
				called = true;
				return Task.CompletedTask;
			};

			await projector.HandleSubscriberEventAsync(subEvent, token);

			Assert.True(called);
		}
	}
}
