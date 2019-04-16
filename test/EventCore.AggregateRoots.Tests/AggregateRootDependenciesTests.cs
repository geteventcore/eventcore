using System;
using EventCore.EventSourcing;
using EventCore.Utilities;
using Moq;
using Xunit;

namespace EventCore.AggregateRoots.Tests
{
	public class AggregateRootDependenciesTests
	{
		[Fact]
		public void construct()
		{
			var logger = new Mock<IStandardLogger>();
			var stateFactory = new Mock<IAggregateRootStateFactory<IAggregateRootState>>();
			var streamIdBuilder = new Mock<IStreamIdBuilder>();
			var streamClient = new Mock<IStreamClient>();
			var resolver = new Mock<IBusinessEventResolver>();
			var handlerFactory = new Mock<ICommandHandlerFactory<IAggregateRootState>>();
			var serializedStateRepo = new Mock<ISerializedAggregateRootStateRepo>();

			var dependencies = new AggregateRootDependencies<IAggregateRootState>(
				logger.Object, stateFactory.Object, streamIdBuilder.Object, streamClient.Object,
				resolver.Object, handlerFactory.Object, serializedStateRepo.Object
			);

			Assert.Equal(logger.Object, dependencies.Logger);
			Assert.Equal(stateFactory.Object, dependencies.StateFactory);
			Assert.Equal(streamIdBuilder.Object, dependencies.StreamIdBuilder);
			Assert.Equal(streamClient.Object, dependencies.StreamClient);
			Assert.Equal(resolver.Object, dependencies.Resolver);
			Assert.Equal(handlerFactory.Object, dependencies.HandlerFactory);
			Assert.Equal(serializedStateRepo.Object, dependencies.SerializedStateRepo);
		}
	}
}
