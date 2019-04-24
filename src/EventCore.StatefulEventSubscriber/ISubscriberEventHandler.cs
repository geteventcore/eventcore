using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulEventSubscriber
{
	public interface ISubscriberEventHandler
	{
		Task HandleAsync(SubscriberEvent subscriberEvent, CancellationToken cancellationToken);
	}
}
