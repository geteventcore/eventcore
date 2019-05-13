namespace EventCore.Samples.SimpleEventStore.EventStoreDb.DbModels
{
	public class SubscriptionFilterDbModel
	{
		public string SubscriptionName { get; set; }
		public string StreamIdPrefix { get; set; }
	}
}
