using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public interface ISortingQueue
	{
		Task AwaitEnqueueSignalAsync();
		Task EnqueueWithWaitAsync(SubscriberEvent subscriberEvent, CancellationToken cancellationToken);
		bool TryDequeue(out SubscriberEvent subscriberEvent);
	}
}
