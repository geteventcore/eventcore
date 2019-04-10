using EventCore.EventSourcing;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulEventSubscriber
{
	public interface IHandlingQueue
	{
		Task AwaitEnqueueSignalAsync();
		bool IsEventsAvailable { get; }
		Task EnqueueWithWaitAsync(string parallelKey, SubscriberEvent subscriberEvent, CancellationToken cancellationToken);
		HandlingQueueItem TryDequeue(IList<string> filterOutParallelKeys);
	}
}
