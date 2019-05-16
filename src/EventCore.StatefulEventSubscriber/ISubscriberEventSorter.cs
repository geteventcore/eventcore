namespace EventCore.StatefulEventSubscriber
{
	public interface ISubscriberEventSorter
	{
		string SortSubscriberEventToParallelKey(SubscriberEvent subscriberEvent);
	}
}
