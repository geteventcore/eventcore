using EventCore.EventSourcing;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.EventSourcing.StatefulSubscriber
{
	public interface IResolutionManager
	{
		Task ReceiveStreamEventAsync(StreamEvent streamEvent, long firstPositionInStream, CancellationToken cancellationToken);
		Task ManageAsync(CancellationToken cancellationToken);
	}
}
