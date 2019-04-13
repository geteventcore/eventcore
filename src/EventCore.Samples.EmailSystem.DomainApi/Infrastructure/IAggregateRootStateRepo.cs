using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.DomainApi.Infrastructure
{
	public interface IAggregateRootStateRepo
	{
		Task<string> LoadStateAsync(string aggregateRootName, string aggregateRootId);
		Task SaveStateAsync(string aggregateRootName, string aggregateRootId, string serializedState);
	}
}
