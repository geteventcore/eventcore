using EventCore.EventSourcing;
using Moq;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventCore.AggregateRoots.Tests
{
	public class AggregateRootStateTests
	{
		private class TestState : AggregateRootState,
			IApplyBusinessEvent<IBusinessEvent>
		{
			public Func<string, long, IBusinessEvent, CancellationToken, Task> HandlerDelegate;
			public Func<string, Task> CausalIdAdderDelegate;

			public TestState(IBusinessEventResolver eventResolver) : base(eventResolver) { }

			protected override Task AddCausalIdToHistoryAsync(string causalId) => CausalIdAdderDelegate(causalId);
			public override Task<bool> IsCausalIdInHistoryAsync(string causalId) => throw new NotImplementedException();

			public Task ApplyBusinessEventAsync(string streamId, long position, IBusinessEvent e, CancellationToken cancellationToken)
			{
				return HandlerDelegate(streamId, position, e, cancellationToken);
			}
		}

		[Fact]
		public async Task invoke_typed_business_event_handler()
		{
			var streamId = "s";
			var position = 5;
			var mockEventResolver = new Mock<IBusinessEventResolver>();
			var mockBusinessEvent = new Mock<IBusinessEvent>();
			var cancellationToken = CancellationToken.None;
			var state = new TestState(mockEventResolver.Object);
			var called = false;

			state.HandlerDelegate = (pStreamId, pPos, pEvent, pToken) =>
			{
				Assert.Equal(streamId, pStreamId);
				Assert.Equal(position, pPos);
				Assert.Equal(mockBusinessEvent.Object, pEvent);
				Assert.Equal(cancellationToken, pToken);
				called = true;
				return Task.CompletedTask;
			};

			await state.InvokeTypedBusinessEventHandlerAsync(streamId, position, mockBusinessEvent.Object, cancellationToken);

			Assert.True(called);
		}

		[Fact]
		public async Task apply_stream_event_when_event_type_is_resolvable_and_update_state()
		{
			var mockEventResolver = new Mock<IBusinessEventResolver>();
			var mockBusinessEvent = new Mock<IBusinessEvent>();
			var streamId = "s";
			var position = 5;
			var eventType = "x";
			var eventData = Encoding.Unicode.GetBytes("data");
			var streamEvent = new StreamEvent(streamId, position, null, eventType, eventData);
			var causalId = "c";
			var cancellationToken = CancellationToken.None;
			var state = new TestState(mockEventResolver.Object);
			var handlerCalled = false;
			var addCausalIdCalled = false;

			mockEventResolver.Setup(x => x.CanResolve(eventType)).Returns(true);
			mockEventResolver.Setup(x => x.Resolve(eventType, eventData)).Returns(mockBusinessEvent.Object);
			mockBusinessEvent.Setup(x => x.GetCausalId()).Returns(causalId);

			state.HandlerDelegate = (pStreamId, pPos, pEvent, pToken) =>
			{
				Assert.Equal(streamId, pStreamId);
				Assert.Equal(position, pPos);
				Assert.Equal(mockBusinessEvent.Object, pEvent);
				Assert.Equal(cancellationToken, pToken);
				handlerCalled = true;
				return Task.CompletedTask;
			};

			state.CausalIdAdderDelegate = (pCausalId) =>
			{
				Assert.Equal(causalId, pCausalId);
				addCausalIdCalled = true;
				return Task.FromResult(true);
			};

			await state.ApplyStreamEventAsync(streamEvent, CancellationToken.None);

			Assert.True(handlerCalled);
			Assert.True(addCausalIdCalled); // Added causal id to history?
			Assert.Equal(position, state.StreamPositionCheckpoint); // Updated stream checkpoint?
		}

		[Fact]
		public async Task not_apply_stream_event_but_update_stream_position_when_event_type_is_not_resolvable()
		{
			var mockEventResolver = new Mock<IBusinessEventResolver>();
			var called = false;
			var state = new TestState(mockEventResolver.Object);
			var position = 5;
			var eventType = "x";
			var streamEvent = new StreamEvent(null, position, null, eventType, null);

			mockEventResolver.Setup(x => x.CanResolve(eventType)).Returns(false);

			state.HandlerDelegate = (_1, _2, _3, _4) =>
			{
				called = true;
				return Task.CompletedTask;
			};

			await state.ApplyStreamEventAsync(streamEvent, CancellationToken.None);

			Assert.False(called);
			Assert.Equal(position, state.StreamPositionCheckpoint); // Updated stream checkpoint?
		}
	}
}
