using System.Collections.Generic;

namespace EventCore.AggregateRoots.SerializableState
{
	public class SerializableData<TInternalState>
	{
		public readonly long? StreamPositionCheckpoint;
		public readonly IList<string> CausalIdHistory;
		public readonly TInternalState InternalState;

		public SerializableData(long? streamPositionCheckpoint, IList<string> causalIdHistory, TInternalState internalState)
		{
			StreamPositionCheckpoint = streamPositionCheckpoint;
			CausalIdHistory = causalIdHistory;
			InternalState = internalState;
		}
	}
}
