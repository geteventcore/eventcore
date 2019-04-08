using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulEventSubscriber
{
	public interface ISortingManager
	{
		Task ReceiveSubscriberEventAsync(SubscriberEvent subscriberEvent, CancellationToken cancellationToken);
		Task ManageAsync(CancellationToken cancellationToken);
	}
}
