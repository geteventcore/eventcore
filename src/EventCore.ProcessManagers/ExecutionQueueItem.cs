using System;
using System.Threading.Tasks;

namespace EventCore.ProcessManagers
{
	public class ExecutionQueueItem
	{
		public readonly string ExecutionId; // Unique id to separate duplicate process type/id pairs.
		public readonly ProcessIdentifier ProcessId;
		public readonly DateTime? ExecuteAfterUtc;

		public ExecutionQueueItem(string executionId, ProcessIdentifier processId, DateTime? executeAfterUtc)
		{
			ExecutionId = executionId;
			ProcessId = processId;
			ExecuteAfterUtc = executeAfterUtc;
		}
	}
}
