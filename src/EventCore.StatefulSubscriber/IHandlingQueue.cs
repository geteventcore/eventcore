using EventCore.EventSourcing;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public interface IHandlingQueue
	{
		ManualResetEventSlim EnqueueTrigger { get; }
		bool IsEventsAvailable { get; }
		Task EnqueueWithWaitAsync(string parallelKey, SubscriberEvent subscriberEvent, CancellationToken cancellationToken);
		HandlingQueueItem TryDequeue(IList<string> filterOutParallelKeys);
	}
}
