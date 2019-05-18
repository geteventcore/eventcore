using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public interface IHandlingManagerHandlerRunner
	{
		Task TryRunHandlerAsync(SubscriberEvent subscriberEvent, CancellationToken cancellationToken);
	}
}
