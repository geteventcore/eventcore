using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventCore.ProcessManagers
{
	public interface IProcessManagerStateRepo
	{
		Task AddOrUpdateQueuedProcessAsync(string processType, string processId, DateTime? dueUtc, DateTime defaultDueUtc, int executionAttempts);
		Task RemoveQueuedProcessAsync(string processType, string processId);
		Task<QueuedProcess> GetNextQueuedProcessAsync(DateTime dueBeforeUtc, int maxAttempts);
	}
}
