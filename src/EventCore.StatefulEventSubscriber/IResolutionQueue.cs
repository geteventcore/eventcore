using EventCore.EventSourcing;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulEventSubscriber
{
	public interface IResolutionQueue
	{
		Task AwaitEnqueueSignalAsync();
		Task EnqueueWithWaitAsync(StreamEvent streamEvent, CancellationToken cancellationToken);
		StreamEvent TryDequeue();
	}
}
