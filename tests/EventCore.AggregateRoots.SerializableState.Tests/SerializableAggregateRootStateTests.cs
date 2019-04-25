using EventCore.EventSourcing;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventCore.AggregateRoots.SerializableState.Tests
{
	public class SerializableAggregateRootStateTests
	{
		private class TestInternalState { }

		private class TestState : SerializableAggregateRootState<TestInternalState>
		{
			protected override TestInternalState _internalState { get => InternalState; set => InternalState = value; }

			public TestInternalState InternalState { get; set; }
			public List<string> CausalIdHistory { get => _causalIdHistory; }
			public int MaxCausalIdHistory { get => _maxCausalIdHistory; }

			public TestState(IBusinessEventResolver resolver, IGenericBusinessEventHydrator genericHydrator, ISerializableAggregateRootStateObjectRepo repo, string regionId, string context, string aggregateRootName, string aggregateRootId) : base(resolver, genericHydrator, repo, regionId, context, aggregateRootName, aggregateRootId)
			{
			}

			public void SetStreamPositionCheckpoint(long? value) => StreamPositionCheckpoint = value;
		}

		[Fact]
		public async Task add_causal_id_to_history_case_insensitive()
		{
			var causalIdLower = "abc";
			var causalIdUpper = "ABC";

			var state = new TestState(null, null, null, null, null, null, null);

			Assert.False(await state.IsCausalIdInHistoryAsync(causalIdLower));
			await state.AddCausalIdToHistoryAsync(causalIdUpper);
			Assert.True(await state.IsCausalIdInHistoryAsync(causalIdLower));
		}

		[Fact]
		public async Task honor_max_causal_id_history()
		{
			var state = new TestState(null, null, null, null, null, null, null);
			var causalIdNext = "next";

			for (var i = 1; i <= state.MaxCausalIdHistory; i++)
			{
				await state.AddCausalIdToHistoryAsync("c" + i);
			}

			Assert.Equal(state.MaxCausalIdHistory, state.CausalIdHistory.Count);
			await state.AddCausalIdToHistoryAsync(causalIdNext);
			Assert.True(await state.IsCausalIdInHistoryAsync(causalIdNext));
			Assert.Equal(state.MaxCausalIdHistory, state.CausalIdHistory.Count);
		}

		[Fact]
		public async Task hydrate_from_checkpoint()
		{
			var regionId = "x";
			var context = "ctx";
			var aggregateRootName = "ar";
			var aggregateRootId = "1";
			var streamPositionCheckpoint = 1;
			var mockRepo = new Mock<ISerializableAggregateRootStateObjectRepo>();
			var cancelSource = new CancellationTokenSource();
			var baseHydrationCalled = false;

			Func<Func<StreamEvent, Task>, Task> streamLoaderAsync = (_1) => { baseHydrationCalled = true; return Task.CompletedTask; };

			var state = new TestState(null, null, mockRepo.Object, regionId, context, aggregateRootName, aggregateRootId);

			mockRepo
				.Setup(x => x.SaveAsync(regionId, context, aggregateRootName, aggregateRootId, It.IsAny<SerializableAggregateRootStateObject<TestInternalState>>()))
				.Callback<string, string, string, string, SerializableAggregateRootStateObject<TestInternalState>>((_1, _2, _3, _4, stateObj) =>
				{
					Assert.Equal(streamPositionCheckpoint, stateObj.StreamPositionCheckpoint);
					Assert.Equal(state.CausalIdHistory, stateObj.CausalIdHistory); // Ref equality, value will be null.
					Assert.Equal(state.InternalState, stateObj.InternalState); // Ref equality, value will be null.
				})
				.Returns(Task.CompletedTask);

			state.SetStreamPositionCheckpoint(1);
			await state.HydrateFromCheckpointAsync(streamLoaderAsync, cancelSource.Token);

			Assert.True(baseHydrationCalled);
			mockRepo.Verify(x => x.SaveAsync(regionId, context, aggregateRootName, aggregateRootId, It.IsAny<SerializableAggregateRootStateObject<TestInternalState>>()));
		}

		[Fact]
		public async Task initialize_but_not_set_iternal_state_when_load_null()
		{
			var regionId = "x";
			var context = "ctx";
			var aggregateRootName = "ar";
			var aggregateRootId = "1";
			var mockRepo = new Mock<ISerializableAggregateRootStateObjectRepo>();

			mockRepo.Setup(x => x.LoadAsync<TestInternalState>(regionId, context, aggregateRootName, aggregateRootId)).ReturnsAsync((SerializableAggregateRootStateObject<TestInternalState>)null);

			var state = new TestState(null, null, mockRepo.Object, regionId, context, aggregateRootName, aggregateRootId);

			await state.InitializeAsync(CancellationToken.None); // Will throw null reference exception if proceeds into body code.
		}

		[Fact]
		public async Task initialize_and_set_internal_state()
		{
			var regionId = "x";
			var context = "ctx";
			var aggregateRootName = "ar";
			var aggregateRootId = "1";
			var mockRepo = new Mock<ISerializableAggregateRootStateObjectRepo>();
			var streamPositionCheckpoint = 1;
			var causalId = "abc";
			var internalState = new TestInternalState();
			var stateObj = new SerializableAggregateRootStateObject<TestInternalState>(streamPositionCheckpoint, new List<string>() { causalId }, internalState);

			mockRepo
				.Setup(x => x.LoadAsync<TestInternalState>(regionId, context, aggregateRootName, aggregateRootId))
				.ReturnsAsync(stateObj);

			var state = new TestState(null, null, mockRepo.Object, regionId, context, aggregateRootName, aggregateRootId);

			await state.InitializeAsync(CancellationToken.None);

			Assert.Equal(streamPositionCheckpoint, state.StreamPositionCheckpoint);
			Assert.Contains(causalId, state.CausalIdHistory);
			Assert.Equal(state.InternalState, internalState);
		}
	}
}
