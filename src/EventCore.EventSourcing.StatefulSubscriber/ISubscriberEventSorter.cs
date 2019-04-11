namespace EventCore.EventSourcing.StatefulSubscriber
{
	public interface ISubscriberEventSorter
	{
		string SortToParallelKey(SubscriberEvent subscriberEvent);
	}
}
