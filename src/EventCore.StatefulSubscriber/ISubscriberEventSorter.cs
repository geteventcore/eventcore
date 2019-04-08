namespace EventCore.StatefulSubscriber
{
	public interface ISubscriberEventSorter
	{
		string SortToParallelKey(SubscriberEvent subscriberEvent);
	}
}
