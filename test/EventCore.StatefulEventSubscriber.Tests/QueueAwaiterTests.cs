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
		public void set_enqueue_signal()
		{
			var awaiter = new QueueAwaiter();

			awaiter.SetDequeueSignal();

			Assert.True(awaiter.IsDequeueSignalSet);
		}

		[Fact]
		public void set_dequeue_signal()
		{
			var awaiter = new QueueAwaiter();

			awaiter.SetEnqueueSignal();

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

			// Signal should auto reset.
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

			// Signal should auto reset.
			Assert.False(awaiter.IsEnqueueSignalSet);
		}
	}
}
