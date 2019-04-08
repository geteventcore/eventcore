using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public interface ISubscriberEventHandler
	{
		Task HandleAsync(SubscriberEvent subscriberEvent, CancellationToken cancellationToken);
	}
}
