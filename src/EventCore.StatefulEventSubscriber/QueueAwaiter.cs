using EventCore.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulEventSubscriber
{
	public class QueueAwaiter : IQueueAwaiter
	{
		private readonly ManualResetEventSlim _dequeueSignal = new ManualResetEventSlim(false);
		private readonly ManualResetEventSlim _enqueueSignal = new ManualResetEventSlim(false);

		// For testing.
		public bool IsDequeueSignalSet { get => _dequeueSignal.IsSet; }
		public bool IsEnqueueSignalSet { get => _enqueueSignal.IsSet; }

		public QueueAwaiter()
		{
		}

		public async Task AwaitDequeueSignalAsync()
		{
			await _dequeueSignal.WaitHandle.AsTask();
			_dequeueSignal.Reset(); // Auto reset.
		}

		public async Task AwaitEnqueueSignalAsync()
		{

			await _enqueueSignal.WaitHandle.AsTask();
			_enqueueSignal.Reset(); // Auto reset.
		}

		public void SetDequeueSignal() => _dequeueSignal.Set();
		public void SetEnqueueSignal() => _enqueueSignal.Set();
	}
}
