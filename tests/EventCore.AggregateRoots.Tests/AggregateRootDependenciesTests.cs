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
			var mockStateRepo = new Mock<IAggregateRootStateRepo<IAggregateRootState>>();
			var mockStreamIdBuilder = new Mock<IStreamIdBuilder>();
			var mockStreamClientFactory = new Mock<IStreamClientFactory>();
			var mockEventResolver = new Mock<IBusinessEventResolver>();

			var dependencies = new AggregateRootDependencies<IAggregateRootState>(
				mockLogger.Object, mockStateRepo.Object, mockStreamIdBuilder.Object,
				mockStreamClientFactory.Object, mockEventResolver.Object
			);

			Assert.Equal(mockLogger.Object, dependencies.Logger);
			Assert.Equal(mockStateRepo.Object, dependencies.StateRepo);
			Assert.Equal(mockStreamIdBuilder.Object, dependencies.StreamIdBuilder);
			Assert.Equal(mockStreamClientFactory.Object, dependencies.StreamClientFactory);
			Assert.Equal(mockEventResolver.Object, dependencies.EventResolver);
		}
	}
}
