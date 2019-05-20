namespace EventCore.Samples.Ecommerce.ServiceApi.Settings
{
	public class EventSourcingSettings
	{
		public int StreamReadBatchSize { get; set; }
		public int ReconnectDelaySeconds { get; set; }
	}
}

