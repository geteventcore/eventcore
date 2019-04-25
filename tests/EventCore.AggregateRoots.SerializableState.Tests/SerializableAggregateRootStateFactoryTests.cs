using EventCore.EventSourcing;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventCore.AggregateRoots.SerializableState.Tests
{
	public class SerializableAggregateRootStateFactoryTests
	{
		public class TestInternalState { } // Must be public for Moq.

		[Fact]
		public async Task create_and_load_to_checkpoint()
		{
			var regionId = "x";
			var context = "ctx";
			var aggregateRootName = "ar";
			var aggregateRootId = "1";
			var mockResolver = new Mock<IBusinessEventResolver>();
			var mockHydrator = new Mock<IGenericBusinessEventHydrator>();
			var mockRepo = new Mock<ISerializableAggregateRootStateObjectRepo>();
			var mockState = new Mock<ISerializableAggregateRootState<TestInternalState>>();
			var cancelSource = new CancellationTokenSource();

			Func<IBusinessEventResolver, IGenericBusinessEventHydrator, ISerializableAggregateRootStateObjectRepo, string, string, string, string, ISerializableAggregateRootState<TestInternalState>> stateConstructor =
			(pResolver, pHydrator, pRepo, pRegionId, pContext, pAggregateRootName, pAggregateRootId) =>
			{
				if (pResolver != mockResolver.Object || pHydrator != mockHydrator.Object || pRepo != mockRepo.Object
					|| pRegionId != regionId || pContext != context || pAggregateRootName != aggregateRootName || pAggregateRootId != aggregateRootId)
				{
					throw new Exception();
				}
				return mockState.Object;
			};

			mockState.Setup(x => x.InitializeAsync(cancelSource.Token)).Returns(Task.CompletedTask);

			var factory = new SerializableAggregateRootStateFactory<ISerializableAggregateRootState<TestInternalState>, TestInternalState>(mockResolver.Object, mockHydrator.Object, mockRepo.Object, stateConstructor);
			var createdState = await factory.CreateAndLoadToCheckpointAsync(regionId, context, aggregateRootName, aggregateRootId, cancelSource.Token);

			Assert.Equal(mockState.Object, createdState);
			mockState.Verify(x => x.InitializeAsync(cancelSource.Token));
		}
	}
}
