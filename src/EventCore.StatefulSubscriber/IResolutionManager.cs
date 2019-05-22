using EventCore.EventSourcing;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public interface IResolutionManager
	{
		Task ReceiveStreamEventAsync(string regionId, StreamEvent streamEvent, long firstPositionInStream, CancellationToken cancellationToken);
		Task ManageAsync(CancellationToken cancellationToken);
	}
}
