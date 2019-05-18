namespace EventCore.StatefulSubscriber
{
	public class HandlingQueueItem
	{
		public readonly string ParallelKey;
		public readonly SubscriberEvent SubscriberEvent;

		public HandlingQueueItem(string parallelKey, SubscriberEvent subscriberEvent)
		{
			ParallelKey = parallelKey;
			SubscriberEvent = subscriberEvent;
		}
	}
}
