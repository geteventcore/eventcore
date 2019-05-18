using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public interface ISubscriberEventHandler
	{
		Task HandleSubscriberEventAsync(SubscriberEvent subscriberEvent, CancellationToken cancellationToken);
	}
}
