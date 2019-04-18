using System.Threading.Tasks;

namespace EventCore.AggregateRoots.SerializableState
{
	public interface ISerializableAggregateRootStateObjectRepo
	{
		Task<SerializableAggregateRootStateObject> LoadAsync<T>(string regionId, string context, string aggregateRootName, string aggregateRootId, T type);
		Task SaveAsync(string regionId, string context, string aggregateRootName, string aggregateRootId, SerializableAggregateRootStateObject state);
	}
}
