using Microsoft.Extensions.Configuration;

namespace EventCore.Samples.Ecommerce.ServiceApi.Settings
{
	public class ProjectorsSettings
	{
		public static ProjectorsSettings Get(IConfiguration config) => config.GetSection("Services:Projectors").Get<ProjectorsSettings>();

		public int MaxResolutionQueueSize { get; set; }
		public int MaxSortingQueueSize { get; set; }
		public int MaxHandlingQueuesSharedSize { get; set; }
		public int MaxParallelHandlerExecutions { get; set; }
		public SubscriptionStreamSettings[] SubscriptionStreams { get; set; }
	}
}

