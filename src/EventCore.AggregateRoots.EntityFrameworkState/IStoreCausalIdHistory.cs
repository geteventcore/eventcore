using System.Threading.Tasks;

namespace EventCore.AggregateRoots.EntityFrameworkState
{
	public interface IStoreCausalIdHistory
	{
		Task AddCausalIdToHistoryIfNotExistsAsync(string causalId);
		Task<bool> ExistsCausalIdInHistoryAsync(string causalId);
	}
}
