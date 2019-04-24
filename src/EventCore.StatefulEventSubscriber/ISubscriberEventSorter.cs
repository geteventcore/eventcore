namespace EventCore.StatefulEventSubscriber
{
	public interface ISubscriberEventSorter
	{
		string SortToParallelKey(SubscriberEvent subscriberEvent);
	}
}
