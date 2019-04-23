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
		private class TestBusinessEvent1 : BusinessEvent
		{
			public TestBusinessEvent1(BusinessEventMetadata metadata) : base(metadata) { }
		}

		private class TestState : AggregateRootState,
			IApplyBusinessEvent<TestBusinessEvent1>
		{
			public Func<string, long, TestBusinessEvent1, CancellationToken, Task> ApplyBusinessEventAsyncDelegate;

			public TestState(IBusinessEventResolver resolver) : base(resolver)
			{
			}

			public Task ApplyBusinessEventAsync(string streamId, long position, TestBusinessEvent1 e, CancellationToken cancellationToken)
				=> ApplyBusinessEventAsyncDelegate(streamId, position, e, cancellationToken);

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
			var resolvedEvent = new TestBusinessEvent1(BusinessEventMetadata.Empty);
			var mockResolver = new Mock<IBusinessEventResolver>();
			var cancelSource = new CancellationTokenSource();
			var state = new TestState(mockResolver.Object);
			var applyCalled = false;

			Func<Func<StreamEvent, Task>, Task> streamLoaderAsync = (eventReceiverAsync) => eventReceiverAsync(streamEvent);

			mockResolver.Setup(x => x.Resolve(It.IsAny<string>(), It.IsAny<byte[]>())).Returns(resolvedEvent);
			mockResolver.Setup(x => x.CanResolve(eventType)).Returns(true);

			state.ApplyBusinessEventAsyncDelegate = (pStreamId, pPosition, pEvent, pToken) =>
			{
				Assert.Equal(streamId, pStreamId);
				Assert.Equal(position, pPosition);
				Assert.Equal(resolvedEvent, pEvent);
				Assert.Equal(cancelSource.Token, pToken);
				applyCalled = true;
				return Task.CompletedTask;
			};

			Assert.Null(state.StreamPositionCheckpoint);

			await state.HydrateFromCheckpointAsync(streamLoaderAsync, cancelSource.Token);

			Assert.True(applyCalled);
			Assert.Equal(position, state.StreamPositionCheckpoint);
		}

		[Fact]
		public async Task hydrate_from_checkpoint_and_set_new_checkpoint_when_event_not_resolvable()
		{
			var streamId = "s";
			var eventType = "x";
			var position = 5;
			var streamEvent = new StreamEvent(streamId, position, null, eventType, new byte[] { });
			var resolvedEvent = new TestBusinessEvent1(BusinessEventMetadata.Empty);
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
			var position = 5;
			var e = new TestBusinessEvent1(BusinessEventMetadata.Empty);
			var mockResolver = new Mock<IBusinessEventResolver>();
			var state = new TestState(mockResolver.Object);
			var cancelSource = new CancellationTokenSource();
			var applyCalled = false;

			state.ApplyBusinessEventAsyncDelegate = (pStreamId, pPosition, pEvent, pToken) =>
			{
				Assert.Equal(streamId, pStreamId);
				Assert.Equal(position, pPosition);
				Assert.Equal(e, pEvent);
				Assert.Equal(cancelSource.Token, pToken);
				applyCalled = true;
				return Task.CompletedTask;
			};

			await AggregateRootState.ApplyGenericBusinessEventAsync(state, streamId, position, e, cancelSource.Token);

			Assert.True(applyCalled);
		}
	}
}
