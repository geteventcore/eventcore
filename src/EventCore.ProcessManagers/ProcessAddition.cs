namespace EventCore.ProcessManagers
{
	public class ProcessAddition
	{
		public readonly int QueueProcessingBatchSize;

		public ProcessAddition(int queueProcessingBatchSize)
		{
			QueueProcessingBatchSize = queueProcessingBatchSize;
		}
	}
}
