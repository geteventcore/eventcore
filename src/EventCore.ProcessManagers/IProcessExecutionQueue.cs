using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventCore.ProcessManagers
{
	public interface IProcessExecutionQueue
	{
		Task EnqueueExecutionAsync(string executionId, ProcessIdentifier processId, DateTime executeAfterUtc);
		Task<ExecutionQueueItem> GetNextExecutionAsync(DateTime maxExecuteAfterUtc, IEnumerable<ProcessIdentifier> excludeProcessIds);
		Task CompleteExecutionAsync(string executionId, string errorMessage);
	}
}
