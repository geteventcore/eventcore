using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public interface ISerializedAggregateRootStateRepo
	{
		Task<string> LoadStateAsync(string aggregateRootName, string aggregateRootId);
		Task SaveStateAsync(string aggregateRootName, string aggregateRootId, string serializedState);
	}
}
