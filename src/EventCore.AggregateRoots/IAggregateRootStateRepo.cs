using System.Threading;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public interface IAggregateRootStateRepo<TState> where TState : IAggregateRootState
	{
		Task<TState> LoadAsync(string regionId, string streamId, CancellationToken cancellationToken); // Load latest state from store and/or latest events in stream.
	}
}
