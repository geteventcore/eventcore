using System.Collections.Generic;

namespace EventCore.AggregateRoots.SerializableState
{
	public class SerializableAggregateRootStateObject<TInternalState>
	{
		public readonly long? StreamPositionCheckpoint;
		public readonly IList<string> CausalIdHistory;
		public readonly object InternalState;

		public SerializableAggregateRootStateObject(long? streamPositionCheckpoint, IList<string> causalIdHistory, TInternalState internalState)
		{
			StreamPositionCheckpoint = streamPositionCheckpoint;
			CausalIdHistory = causalIdHistory;
			InternalState = internalState;
		}
	}
}
