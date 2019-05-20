namespace EventCore.Samples.Ecommerce.ServiceApi.Settings
{
	public class ProjectorsSharedSettings
	{
		public int MaxResolutionQueueSize { get; set; }
		public int MaxSortingQueueSize { get; set; }
		public int MaxHandlingQueuesSharedSize { get; set; }
		public int MaxParallelHandlerExecutions { get; set; }
		public SubscriptionStream[] SubscriptionStreams { get; set; }

		public class SubscriptionStream
		{
			public string RegionId { get; set; }
			public string StreamId { get; set; }
		}
	}
}

