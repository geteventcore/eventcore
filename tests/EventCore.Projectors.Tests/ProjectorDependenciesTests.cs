using EventCore.EventSourcing;
using EventCore.StatefulSubscriber;
using EventCore.Utilities;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace EventCore.Projectors.Tests
{
	public class ProjectorDependenciesTests
	{
		[Fact]
		public void construct()
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
			
			Assert.Equal(mockLogger.Object, dependencies.Logger);
			Assert.Equal(mockSubFactory.Object, dependencies.SubscriberFactory);
			Assert.Equal(subFactoryOptions, dependencies.SubscriberFactoryOptions);
			Assert.Equal(mockStreamClientFactory.Object, dependencies.StreamClientFactory);
			Assert.Equal(mockStreamStateRepo.Object, dependencies.StreamStateRepo);
			Assert.Equal(mockEventResolver.Object, dependencies.EventResolver);
			Assert.Equal(streamIds, dependencies.SubscriptionStreamIds);
		}
	}
}
