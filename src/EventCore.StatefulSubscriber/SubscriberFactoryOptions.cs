namespace EventCore.StatefulSubscriber
{
	public class SubscriberFactoryOptions
	{
		public readonly int MaxResolutionQueueSize;
		public readonly int MaxSortingQueueSize;
		public readonly int MaxHandlingQueuesSharedSize;
		public readonly int MaxParallelHandlerExecutions;

		public SubscriberFactoryOptions(int maxResolutionQueueSize, int maxSortingQueueSize, int maxHandlingQueuesSharedSize, int maxParallelHandlerExecutions)
		{
			MaxResolutionQueueSize = maxResolutionQueueSize;
			MaxSortingQueueSize = maxSortingQueueSize;
			MaxHandlingQueuesSharedSize = maxHandlingQueuesSharedSize;
			MaxParallelHandlerExecutions = maxParallelHandlerExecutions;
		}
	}
}
