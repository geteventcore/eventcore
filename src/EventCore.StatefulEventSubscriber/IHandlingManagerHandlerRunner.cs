using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulEventSubscriber
{
	public interface IHandlingManagerHandlerRunner
	{
		Task TryRunHandlerAsync(SubscriberEvent subscriberEvent, CancellationToken cancellationToken);
	}
}
