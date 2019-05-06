using EventCore.EventSourcing;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public interface IAggregateRootState
	{
		long? StreamPositionCheckpoint { get; } // I.e. the last hydrated event position in the aggregate root stream.
		Task<bool> IsCausalIdInHistoryAsync(string causalId);
		Task ApplyStreamEventAsync(StreamEvent streamEvent, CancellationToken cancellationToken);
	}
}
