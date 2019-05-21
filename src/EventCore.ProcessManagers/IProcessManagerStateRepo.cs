using System;
using System.Threading.Tasks;

namespace EventCore.ProcessManagers
{
	public interface IProcessManagerStateRepo
	{
		Task AddOrUpdateProcessAsync(string type, string processId, DateTime dueUtc);
		Task RemoveProcessAsync(string type, string processId);
	}
}
