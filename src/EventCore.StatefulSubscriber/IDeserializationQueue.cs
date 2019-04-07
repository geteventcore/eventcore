using EventCore.EventSourcing;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public interface IDeserializationQueue
	{
		ManualResetEventSlim EnqueueTrigger { get; }
		Task EnqueueWithWaitAsync(StreamEvent streamEvent, CancellationToken cancellationToken);
		StreamEvent TryDequeue();
	}
}
