namespace EventCore.StatefulSubscriber
{
	public interface ISubscriberEventSorter
	{
		string SortSubscriberEventToParallelKey(SubscriberEvent subscriberEvent);
	}
}
