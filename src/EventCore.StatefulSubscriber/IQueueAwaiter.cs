using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public interface IQueueAwaiter
	{
		Task AwaitEnqueueSignalAsync();
		Task AwaitDequeueSignalAsync();
		void SetEnqueueSignal();
		void SetDequeueSignal();
	}
}
