using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public interface ISubscriberEventHandler
	{
		Task HandleSubscriberEvent(SubscriberEvent subscriberEvent, CancellationToken cancellationToken);
	}
}
