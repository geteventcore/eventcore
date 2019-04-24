using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulEventSubscriber
{
	public interface IQueueAwaiter
	{
		Task AwaitEnqueueSignalAsync();
		Task AwaitDequeueSignalAsync();
		void SetEnqueueSignal();
		void SetDequeueSignal();
	}
}
