using EventCore.EventSourcing;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public interface IAggregateRootState
	{
		long? StreamPositionCheckpoint { get; }
		Task HydrateFromCheckpointAsync(Func<Func<StreamEvent, Task>, Task> streamLoaderAsync, CancellationToken cancellationToken);
		Task<bool> IsCausalIdInHistoryAsync(string causalId);
		Task AddCausalIdToHistoryAsync(string causalId);
	}
}
