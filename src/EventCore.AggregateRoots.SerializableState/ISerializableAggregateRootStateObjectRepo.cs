using System.Threading.Tasks;

namespace EventCore.AggregateRoots.SerializableState
{
	public interface ISerializableAggregateRootStateObjectRepo
	{
		Task<SerializableAggregateRootStateObject<TInternalState>> LoadAsync<TInternalState>(string regionId, string context, string aggregateRootName, string aggregateRootId);
		Task SaveAsync<TInternalState>(string regionId, string context, string aggregateRootName, string aggregateRootId, SerializableAggregateRootStateObject<TInternalState> stateObject);
	}
}
