using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulEventSubscriber
{
	public interface IHandlingManagerAwaiter
	{
		Task AwaitThrottleAsync();
		void ReleaseThrottle();
		Task AwaitHandlerCompletionSignalAsync();
		void SetHandlerCompletionSignal();
		void ResetHandlerCompletionSignal();
	}
}
