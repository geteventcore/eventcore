using System.Threading;
using System.Threading.Tasks;

namespace EventCore.EventSourcing.StatefulSubscriber
{
	public interface IHandlingManagerHandlerRunner
	{
		Task TryRunHandlerAsync(SubscriberEvent subscriberEvent, CancellationToken cancellationToken);
	}
}
