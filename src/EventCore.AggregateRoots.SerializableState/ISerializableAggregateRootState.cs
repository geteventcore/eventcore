using System.Threading;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots.SerializableState
{
	public interface ISerializableAggregateRootState<TInternalState> : IAggregateRootState
	{
		Task InitializeAsync(string regionId, string context, string aggregateRootName, string aggregateRootId, CancellationToken cancellationToken);
	}
}
