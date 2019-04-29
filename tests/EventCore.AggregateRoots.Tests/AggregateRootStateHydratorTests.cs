using EventCore.EventSourcing;
using EventCore.Utilities;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventCore.AggregateRoots.Tests
{
	public class AggregateRootStateHydratorTests
	{
		private class TestState : IAggregateRootState,
			IApplyBusinessEvent<IBusinessEvent>
		{
			public Func<string, long, IBusinessEvent, CancellationToken, Task> ApplyDelegate;

			public long? StreamPositionCheckpoint => throw new NotImplementedException();
			public Task AddCausalIdToHistoryAsync(string causalId) => throw new NotImplementedException();
			public Task<bool> IsCausalIdInHistoryAsync(string causalId) => throw new NotImplementedException();
			public Task HydrateFromCheckpointAsync(Func<Func<StreamEvent, Task>, Task> streamLoaderAsync, CancellationToken cancellationToken) => throw new NotImplementedException();

			public Task ApplyBusinessEventAsync(string streamId, long position, IBusinessEvent e, CancellationToken cancellationToken)
			{
				return ApplyDelegate(streamId, position, e, cancellationToken);
			}
		}

		[Fact]
		public async Task apply_generic_business_event()
		{
			var streamId = "s";
			var position = 1;
			var mockEvent = new Mock<IBusinessEvent>();
			var state = new TestState();
			var hydrator = new AggregateRootStateHydrator();
			var cancelSource = new CancellationTokenSource();
			var applyCalled = false;

			state.ApplyDelegate = (pStreamId, pPosition, pEvent, pToken) =>
			{
				if (pStreamId == streamId && pPosition == position && pEvent == mockEvent.Object && pToken == cancelSource.Token)
				{
					applyCalled = true;
				}
				return Task.CompletedTask;
			};

			await hydrator.ApplyGenericBusinessEventAsync(state, streamId, position, mockEvent.Object, cancelSource.Token);

			Assert.True(applyCalled);
		}
	}
}
