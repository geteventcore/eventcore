using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public interface ISortingManager
	{
		Task ReceiveSubscriberEventAsync(SubscriberEvent subscriberEvent, CancellationToken cancellationToken);
		Task ManageAsync(CancellationToken cancellationToken);
	}
}
