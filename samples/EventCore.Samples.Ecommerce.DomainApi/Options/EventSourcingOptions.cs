namespace EventCore.Samples.Ecommerce.DomainApi.Options
{
	public class EventSourcingOptions
	{
		public int StreamReadBatchSize { get; set; } = 100;

		public EventSourcingOptions()
		{
		}
	}
}