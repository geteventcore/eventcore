using System.Collections.Generic;

namespace EventCore.AggregateRoots.SerializableState
{
	public class SerializableAggregateRootStateObject
	{
		public readonly long? StreamPositionCheckpoint;
		public readonly IList<string> CausalIdHistory;
		public readonly object InternalState;

		public SerializableAggregateRootStateObject(long? streamPositionCheckpoint, IList<string> causalIdHistory, object internalState)
		{
			StreamPositionCheckpoint = streamPositionCheckpoint;
			CausalIdHistory = causalIdHistory;
			InternalState = internalState;
		}
	}
}
