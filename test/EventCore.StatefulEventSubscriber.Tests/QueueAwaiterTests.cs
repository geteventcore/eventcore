using EventCore.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventCore.StatefulEventSubscriber.Tests
{
	public class QueueAwaiterTests
	{
		[Fact]
		public void construct_with_signals_not_set()
		{
			var awaiter = new QueueAwaiter();

			Assert.False(awaiter.IsDequeueSignalSet);
			Assert.False(awaiter.IsEnqueueSignalSet);
		}

		[Fact]
		public void set_signals()
		{
			var awaiter = new QueueAwaiter();

			awaiter.SetDequeueSignal();
			awaiter.SetEnqueueSignal();

			Assert.True(awaiter.IsDequeueSignalSet);
			Assert.True(awaiter.IsEnqueueSignalSet);
		}

		[Fact]
		public async Task await_dequeue_signal_until_set_then_reset_signal()
		{
			var cts = new CancellationTokenSource(5000);
			var awaiter = new QueueAwaiter();

			var waitTask = awaiter.AwaitDequeueSignalAsync();
			Assert.False(awaiter.IsDequeueSignalSet);

			awaiter.SetDequeueSignal();

			await Task.WhenAny(new[] { waitTask, cts.Token.WaitHandle.AsTask() });
			if (cts.IsCancellationRequested) throw new TimeoutException();

			Assert.False(awaiter.IsDequeueSignalSet);
		}

		[Fact]
		public async Task await_enqueue_signal_until_set_then_reset_signal()
		{
			var cts = new CancellationTokenSource(5000);
			var awaiter = new QueueAwaiter();

			var waitTask = awaiter.AwaitEnqueueSignalAsync();
			Assert.False(awaiter.IsEnqueueSignalSet);

			awaiter.SetEnqueueSignal();

			await Task.WhenAny(new[] { waitTask, cts.Token.WaitHandle.AsTask() });
			if (cts.IsCancellationRequested) throw new TimeoutException();

			Assert.False(awaiter.IsEnqueueSignalSet);
		}
	}
}
