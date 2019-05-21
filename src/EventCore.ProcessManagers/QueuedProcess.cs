using System;
using System.Threading.Tasks;

namespace EventCore.ProcessManagers
{
	public class QueuedProcess
	{
		public readonly string ProcessType;
		public readonly string ProcessId;
		public readonly DateTime DueUtc;

		public QueuedProcess(string processType, string processId, DateTime dueUtc)
		{
			ProcessType = processType;
			ProcessId = processId;
			DueUtc = dueUtc;
		}
	}
}
