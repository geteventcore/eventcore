using EventCore.StatefulSubscriber;

namespace EventCore.Samples.Ecommerce.ProjectorsHost
{
	public class ProjectorSettings
	{
		public int MaxResolutionQueueSize { get; set; }
		public int MaxSortingQueueSize { get; set; }
		public int MaxHandlingQueuesSharedSize { get; set; }
		public int MaxParallelHandlerExecutions { get; set; }
		public string StreamStateBasePath { get; set; }
		public SubscriptionStream[] SubscriptionStreams { get; set; }

		public class SubscriptionStream
		{
			public string RegionId { get; set; }
			public string StreamId { get; set; }
		}
	}
}

