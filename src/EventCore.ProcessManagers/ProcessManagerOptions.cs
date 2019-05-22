namespace EventCore.ProcessManagers
{
	public class ProcessManagerOptions
	{
		public readonly int MaxProcessExecutionAttempts;
		public readonly int MaxParallelProcessExecutions;
		
		public ProcessManagerOptions(int maxProcessExecutionAttempts, int maxParallelProcessExecutions)
		{
			MaxProcessExecutionAttempts = maxProcessExecutionAttempts;
			MaxParallelProcessExecutions = maxParallelProcessExecutions;
		}
	}
}
