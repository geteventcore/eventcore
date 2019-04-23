using System.Collections.Generic;
using Xunit;

namespace EventCore.AggregateRoots.SerializableState.Tests
{
	public class SerializableAggregateRootStateObjectTests
	{
		private class TestInternalState { }

		[Fact]
		public void construct()
		{
			var streamPositionCheckpoint = (long?)1;
			var causalIdHistory = new List<string>();
			var internalState = new TestInternalState();

			var stateObj = new SerializableAggregateRootStateObject<TestInternalState>(streamPositionCheckpoint, causalIdHistory, internalState);

			Assert.Equal(streamPositionCheckpoint, stateObj.StreamPositionCheckpoint);
			Assert.Equal(causalIdHistory, stateObj.CausalIdHistory);
			Assert.Equal(internalState, stateObj.InternalState);
		}
	}
}
