using EventCore.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulEventSubscriber
{
	public class HandlingManagerAwaiter : IHandlingManagerAwaiter
	{
		private readonly SemaphoreSlim _throttle;
		private readonly ManualResetEventSlim _handlerCompletionSignal = new ManualResetEventSlim(false);

		public HandlingManagerAwaiter(int maxParallelHandlerExecutions)
		{
			_throttle = new SemaphoreSlim(maxParallelHandlerExecutions, maxParallelHandlerExecutions);
		}

		public Task AwaitHandlerCompletionSignalAsync() => _handlerCompletionSignal.WaitHandle.AsTask();
		public Task AwaitThrottleAsync() => _throttle.WaitAsync();
		public void ReleaseThrottle() => _throttle.Release();
		public void ResetHandlerCompletionSignal() => _handlerCompletionSignal.Reset();
		public void SetHandlerCompletionSignal() => _handlerCompletionSignal.Set();
	}
}
