using EventCore.StatefulSubscriber;
using EventCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.ProcessManagers
{
	public abstract class ProcessManager : IProcessManager, ISubscriberEventSorter, ISubscriberEventHandler
	{
		// This can be anything except null or empty.
		// Must have a default way to group events to be handled.
		protected const string DEFAULT_PARALLEL_KEY = ".";

		protected readonly IStandardLogger _logger;
		protected readonly ISubscriber _subscriber;
		protected readonly IProcessManagerStateRepo _stateRepo;
		protected readonly ProcessManagerOptions _options;

		protected readonly Dictionary<string, IProcess> _processes = new Dictionary<string, IProcess>();
		protected readonly ManualResetEventSlim _caughtUpSignal = new ManualResetEventSlim(false);
		protected readonly ManualResetEventSlim _enqueueSignal = new ManualResetEventSlim(false);

		protected IDictionary<string, long?> _endsOfSubscription;
		

		public ProcessManager(ProcessManagerDependencies dependencies, ProcessManagerOptions options)
		{
			_logger = dependencies.Logger;
			_subscriber = dependencies.SubscriberFactory.Create(dependencies.Logger, dependencies.StreamClientFactory, dependencies.StreamStateRepo, dependencies.EventResolver, this, this, dependencies.SubscriberFactoryOptions, dependencies.SubscriptionStreamIds);
			_stateRepo = dependencies.ProcessManagerStateRepo;
			_options = options;
		}

		public virtual async Task RunAsync(CancellationToken cancellationToken)
		{
			_endsOfSubscription = await _subscriber.GetEndsOfSubscriptionAsync();
			await Task.WhenAll(new[] { _subscriber.SubscribeAsync(cancellationToken), ManageAsync(cancellationToken) });
		}

		protected virtual async Task ManageAsync(CancellationToken cancellationToken)
		{
			// Wait for hydration to "catch up" before executing processes.
			// This allows the service to replay events and hydrate to the latest state before making decision
			// about what actions to take when a process is executed. Whether the service needs to catch up will depend
			// on the needs of each specific service. For example, if a process is time-based, when the service comes
			// back online after a down period then it will need the latest state to determine if the time-based process
			// should still be executed.
			await Task.WhenAny(new[] { _caughtUpSignal.WaitHandle.AsTask(), cancellationToken.WaitHandle.AsTask() });

			var parallelSignal = new SemaphoreSlim(_options.MaxParallelProcessExecutions, _options.MaxParallelProcessExecutions);

			while (!cancellationToken.IsCancellationRequested)
			{
				// Run processes in order of due date, in parallel.

				var nextProcess = await TryGetNextQueuedProcessAsync(cancellationToken);
				while (nextProcess != null)
				{
					// Throttle the number of processes we execute in parallel.
					await parallelSignal.WaitAsync(cancellationToken);
					if (cancellationToken.IsCancellationRequested)
					{
						break;
					}

					// Execute the process and make sure to release the semaphore.
					var _ = Task.Run(async () =>
					{
						try
						{
							await TryExecuteProcessAsync(nextProcess, cancellationToken);
						}
						catch (Exception ex)
						{
							_logger.LogError(ex, $"Exceeded max execution attempts for {nextProcess.ProcessType} ({nextProcess.ProcessId}). Moving on.");
						}
						finally
						{
							parallelSignal.Release();
						}
					});

					nextProcess = await TryGetNextQueuedProcessAsync(cancellationToken);
				}

				await Task.WhenAny(new[] { _enqueueSignal.WaitHandle.AsTask(), cancellationToken.WaitHandle.AsTask() });
				_enqueueSignal.Reset();
			}
		}

		private async Task TryExecuteProcessAsync(QueuedProcess queuedProcess, CancellationToken cancellationToken)
		{
			var tryCount = _options.MaxProcessExecutionAttempts; // How many times to try in case of transient error?
			var attempt = 1;
			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					var process = _processes[queuedProcess.ProcessType];
					await process.ExecuteAsync(queuedProcess.ProcessId, cancellationToken);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Exception while executing process {queuedProcess.ProcessType} ({queuedProcess.ProcessId}). Assuming transient error and retrying {tryCount - attempt} more times.");

					if (attempt >= tryCount)
					{
						throw ex;
					}

					await Task.Delay(1000 * (3 ^ attempt)); // Crude exponential backoff delay.

					attempt++;
				}
			}
			throw new InvalidOperationException(); // Should never get here.
		}

		private async Task<QueuedProcess> TryGetNextQueuedProcessAsync(CancellationToken cancellationToken)
		{
			var tryCount = 3; // How many times to try in case of transient error?
			var attempt = 1;
			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					return await _stateRepo.GetNextQueuedProcessAsync(DateTime.UtcNow, _options.MaxProcessExecutionAttempts);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Exception while getting queued processes. Assuming transient error and retrying {tryCount - attempt} more times.");

					if (attempt >= tryCount)
					{
						throw ex;
					}

					await Task.Delay(1000 * (3 ^ attempt)); // Crude exponential backoff delay.

					attempt++;
				}
			}
			throw new InvalidOperationException(); // Should never get here.
		}

		protected virtual async Task EnqueueProcessAsync<TProcess>(string processId, DateTime? dueUtc) where TProcess : IProcess
		{
			await _stateRepo.AddOrUpdateQueuedProcessAsync(typeof(TProcess).Name, processId, dueUtc, DateTime.UtcNow, 0);
			_enqueueSignal.Set();
		}

		protected virtual async Task TerminateProcessAsync<TProcess>(string processId) where TProcess : IProcess
		{
			await _stateRepo.RemoveQueuedProcessAsync(typeof(TProcess).Name, processId);
		}

		protected virtual void RegisterProcess<TProcess>(TProcess process) where TProcess : IProcess
		{
			_processes.Add(typeof(TProcess).Name, process);
		}

		// protected virtual bool IsHydrationCaughtUp()
		// {
		// 	// TODO: Check for caught-up condition and trigger signal...
		// 	if (!_caughtUpSignal.IsSet && _end)
		// }

		public virtual string SortSubscriberEventToParallelKey(SubscriberEvent subscriberEvent)
		{
			// Default is no parallel key, i.e. all events from the subscription stream
			// will be handled sequentially with no optimization to execute events in parallel.
			// The implementing class should override this if events can be consumed in parallel,
			// for example, for independent processes that are unrelated to one another.
			return DEFAULT_PARALLEL_KEY; // Sorting keys may not be null or empty.
		}

		public virtual async Task HandleSubscriberEventAsync(SubscriberEvent subscriberEvent, CancellationToken cancellationToken)
		{


			// Does nothing if no handler - event is ignored.
			if (this.GetType().GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IHandleBusinessEvent<>) && x.GetGenericArguments()[0] == subscriberEvent.ResolvedEventType))
			{
				await (Task)this.GetType().InvokeMember("HandleBusinessEventAsync", BindingFlags.InvokeMethod, null, this, new object[] { subscriberEvent.StreamId, subscriberEvent.Position, subscriberEvent.ResolvedEvent, cancellationToken });
			}
		}
	}
}
