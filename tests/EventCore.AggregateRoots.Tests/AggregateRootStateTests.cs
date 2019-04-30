using EventCore.EventSourcing;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventCore.AggregateRoots.Tests
{
	public class AggregateRootStateTests
	{
		private class TestState : AggregateRootState
		{
			public Func<string, long, IBusinessEvent, CancellationToken, Task> ApplyDelegate;

			public TestState(IBusinessEventResolver resolver) : base(resolver) { }
			public override Task AddCausalIdToHistoryAsync(string causalId) => throw new NotImplementedException();
			public override Task<bool> IsCausalIdInHistoryAsync(string causalId) => throw new NotImplementedException();

			public Task ApplyBusinessEventAsync(string streamId, long position, IBusinessEvent e, CancellationToken cancellationToken)
			{
				return ApplyDelegate(streamId, position, e, cancellationToken);
			}
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
			var cancelSource = new CancellationTokenSource();
			var state = new TestState(mockResolver.Object);
			var applyCalled = false;

			Func<Func<StreamEvent, Task>, Task> streamLoaderAsync = (eventReceiverAsync) => eventReceiverAsync(streamEvent);

			mockResolver.Setup(x => x.Resolve(It.IsAny<string>(), It.IsAny<byte[]>())).Returns(mockEvent.Object);
			mockResolver.Setup(x => x.CanResolve(eventType)).Returns(true);

			state.ApplyDelegate = (_1, _2, _3, _4) =>
			{
				applyCalled = true;
				return Task.CompletedTask;
			};

			Assert.Null(state.StreamPositionCheckpoint);

			await state.HydrateFromCheckpointAsync(streamLoaderAsync, cancelSource.Token);

			Assert.Equal(position, state.StreamPositionCheckpoint);
			Assert.True(applyCalled);
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
			var state = new TestState(mockResolver.Object);

			Func<Func<StreamEvent, Task>, Task> streamLoaderAsync = (eventReceiverAsync) => eventReceiverAsync(streamEvent);

			mockResolver.Setup(x => x.CanResolve(eventType)).Returns(false);

			Assert.Null(state.StreamPositionCheckpoint);

			await state.HydrateFromCheckpointAsync(streamLoaderAsync, cancelSource.Token);

			Assert.Equal(position, state.StreamPositionCheckpoint);
		}

		[Fact]
		public async Task apply_generic_business_event()
		{
			var streamId = "s";
			var position = 1;
			var mockEvent = new Mock<IBusinessEvent>();
			var mockResolver = new Mock<IBusinessEventResolver>();
			var state = new TestState(mockResolver.Object);
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

			await state.ApplyGenericBusinessEventAsync(streamId, position, mockEvent.Object, cancelSource.Token);

			Assert.True(applyCalled);
		}
	}
}
