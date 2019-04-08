using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public interface IHandlingManager
	{
		Task ReceiveSubscriberEventAsync(string parallelKey, SubscriberEvent subscriberEvent, CancellationToken cancellationToken);
		Task ManageAsync(CancellationToken cancellationToken);
	}
}
