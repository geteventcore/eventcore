using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventCore.ProcessManagers
{
	public interface IProcessManagerStateRepo
	{
		Task AddOrUpdateQueuedProcessAsync(string processType, string processId, DateTime dueUtc);
		Task RemoveQueuedProcessAsync(string processType, string processId);
		Task<IEnumerable<QueuedProcess>> GetQueuedProcessesAsync(DateTime dueSinceUtc, int batchSize);
	}
}
