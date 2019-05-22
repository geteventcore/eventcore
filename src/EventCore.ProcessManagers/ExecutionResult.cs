namespace EventCore.ProcessManagers
{
	public class ExecutionResult
	{
		public readonly int QueueProcessingBatchSize;

		public ExecutionResult(int queueProcessingBatchSize)
		{
			QueueProcessingBatchSize = queueProcessingBatchSize;
		}
	}
}
