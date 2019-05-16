using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulEventSubscriber
{
	public interface ISubscriberEventHandler
	{
		Task HandleSubscriberEvent(SubscriberEvent subscriberEvent, CancellationToken cancellationToken);
	}
}
