using System;
using System.Threading;
using System.Threading.Tasks;
using EventCore.EventSourcing;
using EventCore.Utilities;
using Xunit;

namespace EventCore.StatefulSubscriber.Tests
{
	public class HandlingManagerAwaiterTests
	{
		[Fact]
		public void construct_with_signal_and_throttle_not_set()
		{
			var maxParallelHandlerExecutions = 5;
			var awaiter = new HandlingManagerAwaiter(maxParallelHandlerExecutions);

			Assert.Equal(maxParallelHandlerExecutions, awaiter.ThrottleCurrentCount);
			Assert.False(awaiter.IsHandlerCompleitionSignalSet);
		}

		[Fact]
		public void set_handler_completion_signal()
		{
			var awaiter = new HandlingManagerAwaiter(1);

			awaiter.SetHandlerCompletionSignal();

			Assert.True(awaiter.IsHandlerCompleitionSignalSet);
		}

		[Fact]
		public void reset_handler_completion_signal()
		{
			var awaiter = new HandlingManagerAwaiter(1);

			awaiter.SetHandlerCompletionSignal();
			Assert.True(awaiter.IsHandlerCompleitionSignalSet);

			awaiter.ResetHandlerCompletionSignal();
			Assert.False(awaiter.IsHandlerCompleitionSignalSet);
		}

		[Fact]
		public async Task await_handler_completion_signal()
		{
			var cts = new CancellationTokenSource(5000);
			var awaiter = new HandlingManagerAwaiter(1);

			var waitTask = awaiter.AwaitHandlerCompletionSignalAsync();

			awaiter.SetHandlerCompletionSignal();

			await Task.WhenAny(new[] { waitTask, cts.Token.WaitHandle.AsTask() });
			if (cts.IsCancellationRequested) throw new TimeoutException();
		}

		[Fact]
		public async Task await_and_release_throttle()
		{
			var cts = new CancellationTokenSource(5000);
			var awaiter = new HandlingManagerAwaiter(1);

			var waitTask1 = awaiter.AwaitThrottleAsync();
			var waitTask2 = awaiter.AwaitThrottleAsync();

			await Task.WhenAny(new[] { waitTask1, cts.Token.WaitHandle.AsTask() });
			if (cts.IsCancellationRequested) throw new TimeoutException();

			Assert.True(waitTask2.Status == TaskStatus.WaitingForActivation);

			awaiter.ReleaseThrottle();

			await Task.WhenAny(new[] { waitTask2, cts.Token.WaitHandle.AsTask() });
			if (cts.IsCancellationRequested) throw new TimeoutException();
		}
	}
}
