using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulEventSubscriber
{
	public interface ISortingQueue
	{
		Task AwaitEnqueueSignalAsync();
		Task EnqueueWithWaitAsync(SubscriberEvent subscriberEvent, CancellationToken cancellationToken);
		SubscriberEvent TryDequeue();
	}
}
