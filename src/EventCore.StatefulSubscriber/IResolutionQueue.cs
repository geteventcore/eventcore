using EventCore.EventSourcing;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public interface IResolutionQueue
	{
		Task AwaitEnqueueSignalAsync();
		Task EnqueueWithWaitAsync(ResolutionStreamEvent streamEvent, CancellationToken cancellationToken);
		bool TryDequeue(out ResolutionStreamEvent streamEvent);
	}
}
