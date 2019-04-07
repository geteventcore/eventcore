using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public interface ISortingQueue
	{
		ManualResetEventSlim EnqueueTrigger { get; }
		Task EnqueueWithWaitAsync(SubscriberEvent subscriberEvent, CancellationToken cancellationToken);
		SubscriberEvent TryDequeue();
	}
}
