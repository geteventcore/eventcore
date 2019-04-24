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
			var mockLogger = new Mock<IStandardLogger>();
			var mockStateFactory = new Mock<IAggregateRootStateFactory<IAggregateRootState>>();
			var mockStreamIdBuilder = new Mock<IStreamIdBuilder>();
			var mockStreamClient = new Mock<IStreamClient>();
			var mockResolver = new Mock<IBusinessEventResolver>();
			var mockHandlerFactory = new Mock<ICommandHandlerFactory<IAggregateRootState>>();

			var dependencies = new AggregateRootDependencies<IAggregateRootState>(
				mockLogger.Object, mockStateFactory.Object, mockStreamIdBuilder.Object, mockStreamClient.Object,
				mockResolver.Object, mockHandlerFactory.Object
			);

			Assert.Equal(mockLogger.Object, dependencies.Logger);
			Assert.Equal(mockStateFactory.Object, dependencies.StateFactory);
			Assert.Equal(mockStreamIdBuilder.Object, dependencies.StreamIdBuilder);
			Assert.Equal(mockStreamClient.Object, dependencies.StreamClient);
			Assert.Equal(mockResolver.Object, dependencies.Resolver);
			Assert.Equal(mockHandlerFactory.Object, dependencies.HandlerFactory);
		}
	}
}
