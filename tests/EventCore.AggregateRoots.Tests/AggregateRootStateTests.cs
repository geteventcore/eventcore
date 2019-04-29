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
	public class AggregateRootStateTests
	{
		private class TestState : AggregateRootState
		{
			public TestState(IBusinessEventResolver resolver, IAggregateRootStateHydrator genericHydrator) : base(resolver, genericHydrator) { }
			public override Task AddCausalIdToHistoryAsync(string causalId) => throw new NotImplementedException();
			public override Task<bool> IsCausalIdInHistoryAsync(string causalId) => throw new NotImplementedException();
		}

		[Fact]
		public async Task hydrate_from_checkpoint_and_set_new_checkpoint_when_event_resolvable()
		{
			var streamId = "s";
			var eventType = "x";
			var position = 5;
			var streamEvent = new StreamEvent(streamId, position, null, eventType, new byte[] { });
			var mockEvent = new Mock<IBusinessEvent>();
			var mockResolver = new Mock<IBusinessEventResolver>();
			var mockHydrator = new Mock<IAggregateRootStateHydrator>();
			var cancelSource = new CancellationTokenSource();
			var state = new TestState(mockResolver.Object, mockHydrator.Object);

			Func<Func<StreamEvent, Task>, Task> streamLoaderAsync = (eventReceiverAsync) => eventReceiverAsync(streamEvent);

			mockResolver.Setup(x => x.Resolve(It.IsAny<string>(), It.IsAny<byte[]>())).Returns(mockEvent.Object);
			mockResolver.Setup(x => x.CanResolve(eventType)).Returns(true);
			mockHydrator
				.Setup(x => x.ApplyGenericBusinessEventAsync(It.IsAny<IAggregateRootState>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IBusinessEvent>(), It.IsAny<CancellationToken>()))
				.Returns(Task.CompletedTask);

			Assert.Null(state.StreamPositionCheckpoint);

			await state.HydrateFromCheckpointAsync(streamLoaderAsync, cancelSource.Token);

			Assert.Equal(position, state.StreamPositionCheckpoint);
			mockHydrator.Verify(x => x.ApplyGenericBusinessEventAsync(state, streamId, position, mockEvent.Object, cancelSource.Token));
		}

		[Fact]
		public async Task hydrate_from_checkpoint_and_set_new_checkpoint_when_event_not_resolvable()
		{
			var streamId = "s";
			var eventType = "x";
			var position = 5;
			var streamEvent = new StreamEvent(streamId, position, null, eventType, new byte[] { });
			var mockResolver = new Mock<IBusinessEventResolver>();
			var cancelSource = new CancellationTokenSource();
			var state = new TestState(mockResolver.Object, null);

			Func<Func<StreamEvent, Task>, Task> streamLoaderAsync = (eventReceiverAsync) => eventReceiverAsync(streamEvent);

			mockResolver.Setup(x => x.CanResolve(eventType)).Returns(false);

			Assert.Null(state.StreamPositionCheckpoint);

			await state.HydrateFromCheckpointAsync(streamLoaderAsync, cancelSource.Token);

			Assert.Equal(position, state.StreamPositionCheckpoint);
		}
	}
}
