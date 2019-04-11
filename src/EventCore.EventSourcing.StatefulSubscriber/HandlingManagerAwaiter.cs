using EventCore.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.EventSourcing.StatefulSubscriber
{
	public class HandlingManagerAwaiter : IHandlingManagerAwaiter
	{
		private readonly SemaphoreSlim _throttle;
		private readonly ManualResetEventSlim _handlerCompletionSignal = new ManualResetEventSlim(false);

		// For testing.
		public int ThrottleCurrentCount { get => _throttle.CurrentCount; }
		public bool IsHandlerCompleitionSignalSet { get => _handlerCompletionSignal.IsSet; }

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
