using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulEventSubscriber
{
	public interface ISortingQueue
	{
		ManualResetEventSlim EnqueueTrigger { get; }
		Task EnqueueWithWaitAsync(SubscriberEvent subscriberEvent, CancellationToken cancellationToken);
		SubscriberEvent TryDequeue();
	}
}
