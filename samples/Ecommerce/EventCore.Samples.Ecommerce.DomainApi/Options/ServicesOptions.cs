namespace EventCore.Samples.Ecommerce.DomainApi.Options
{
	public class ServicesOptions
	{
		public string EventStoreNotificationsHubUrl { get; set; }
		public int StreamReadBatchSize { get; set; } = 100;
		public string AggregateRootStateBasePath { get; set; }
	}
}