using EventCore.StatefulSubscriber;
using EventCore.Utilities;
using System;
using System.Collections.Concurrent;
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
		protected readonly IProcessExecutionQueue _executionQueue;
		protected readonly ProcessManagerOptions _options;

		protected readonly Dictionary<string, IProcess> _processes = new Dictionary<string, IProcess>();
		protected readonly ManualResetEventSlim _caughtUpSignal = new ManualResetEventSlim(false);
		protected readonly ManualResetEventSlim _enqueueSignal = new ManualResetEventSlim(false);

		protected IDictionary<string, long?> _regionalEndsOfSubscription;
		protected readonly ConcurrentDictionary<string, bool> _regionalEndsReached = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

		public ProcessManager(ProcessManagerDependencies dependencies, ProcessManagerOptions options)
		{
			_logger = dependencies.Logger;
			_subscriber = dependencies.SubscriberFactory.Create(dependencies.Logger, dependencies.StreamClientFactory, dependencies.StreamStateRepo, dependencies.EventResolver, this, this, dependencies.SubscriberFactoryOptions, dependencies.SubscriptionStreamIds);
			_executionQueue = dependencies.ExecutionQueue;
			_options = options;
		}

		public virtual async Task RunAsync(CancellationToken cancellationToken)
		{
			_regionalEndsOfSubscription = await _subscriber.GetRegionalEndsOfSubscriptionAsync();
			foreach (var regionId in _regionalEndsOfSubscription.Keys)
			{
				_regionalEndsReached.TryAdd(regionId, false);
			}

			await Task.WhenAll(new[] { _subscriber.SubscribeAsync(cancellationToken), ExecuteProcessesAsync(cancellationToken) });
		}

		protected virtual async Task ExecuteProcessesAsync(CancellationToken cancellationToken)
		{
			// Wait for hydration to "catch up" before executing processes.
			// This allows the service to replay events and hydrate to current state before making decisions
			// about what actions to take when a process is executed. The catch-up point will depend
			// on the needs of each specific service. For example, if a process is time-based, when the service comes
			// back online after a down period then it will need the latest state to determine if the time-based process
			// should still be executed, etc.
			await Task.WhenAny(new[] { _caughtUpSignal.WaitHandle.AsTask(), cancellationToken.WaitHandle.AsTask() });

			var parallelSignal = new SemaphoreSlim(_options.MaxParallelProcessExecutions, _options.MaxParallelProcessExecutions);
			var executingProcessIds = new ConcurrentDictionary<string, ProcessIdentifier>();

			while (!cancellationToken.IsCancellationRequested)
			{
				// Run queued process executions in order of execution date.
				// Execute in parallel based on dinstinct process identifier.
				// I.e. There may be more than one queued execution with the same
				// process identifier (process type and correlation id) - make sure
				// we don't execute those in parallel because those represent sequential
				// processing requests. But other process types or correlation ids may
				// be executed in parallel.

				var nextExecution = await GetNextProcessExecutionAsync(executingProcessIds.Values, cancellationToken);
				while (nextExecution != null)
				{
					// Throttle the number of processes we execute in parallel.
					await parallelSignal.WaitAsync(cancellationToken);
					if (cancellationToken.IsCancellationRequested)
					{
						break;
					}

					// We need to keep track of which process ids are executing because we want to exclude
					// duplicates when fetching the next process execution - see note about parallel execution above.
					executingProcessIds.TryAdd(string.Join(nextExecution.ProcessId.ProcessType, nextExecution.ProcessId.CorrelationId), nextExecution.ProcessId);

					// Execute the process and make sure to release the semaphore.
					var _ = Task.Run(async () =>
					{
						try
						{
							// All processes should be idempotent, capable of being executed
							// more than once in case a transient failure prevents us
							// from marking the execution queue item as complete, but more importantly this is
							// necessary by design to allow event handlers to enqueue the same
							// process type/id more than once during hydration, e.g. when a process
							// depends on multiple events happening in no particular order, they will all
							// kick off the same process to check for the collective result of those events.
							await TryExecuteProcessAsync(nextExecution, cancellationToken);
						}
						finally
						{
							parallelSignal.Release();
							ProcessIdentifier removedPid;
							executingProcessIds.TryRemove(string.Join(nextExecution.ProcessId.ProcessType, nextExecution.ProcessId.CorrelationId), out removedPid);
						}
					});

					nextExecution = await GetNextProcessExecutionAsync(executingProcessIds.Values, cancellationToken);
				}

				await Task.WhenAny(new[] { _enqueueSignal.WaitHandle.AsTask(), cancellationToken.WaitHandle.AsTask() });
				_enqueueSignal.Reset();
			}
		}

		protected virtual async Task TryExecuteProcessAsync(ExecutionQueueItem execution, CancellationToken cancellationToken)
		{
			var tryCount = _options.MaxProcessExecutionAttempts; // How many times to try in case of transient error?
			var attempt = 1;
			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					var process = _processes[execution.ProcessId.ProcessType];
					await process.ExecuteAsync(execution.ProcessId.CorrelationId, cancellationToken);
					await _executionQueue.CompleteExecutionAsync(execution.ExecutionId, null);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Exception while executing process {execution.ProcessId.ProcessType} ({execution.ProcessId.CorrelationId}). Assuming transient error and retrying {tryCount - attempt} more times.");

					if (attempt >= tryCount)
					{
						try
						{
							await _executionQueue.CompleteExecutionAsync(execution.ExecutionId, ex.Message);
						}
						catch (Exception ex2)
						{
							_logger.LogError(ex2, "Secondary exception while trying to complete execution queue item with error information. This execution will remain in the queue.");
							return;
						}
					}

					await Task.Delay(1000 * (3 ^ attempt)); // Crude exponential backoff delay.
					attempt++;
				}
			}
			throw new InvalidOperationException(); // Should never get here.
		}

		protected virtual async Task<ExecutionQueueItem> GetNextProcessExecutionAsync(IEnumerable<ProcessIdentifier> excludeProcessIds, CancellationToken cancellationToken)
		{
			var attempt = 1;
			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					return await _executionQueue.GetNextExecutionAsync(DateTime.UtcNow, excludeProcessIds);
				}
				catch (Exception ex)
				{
					var delaySeconds = 3 ^ attempt; // Crude exponential backoff delay.
					_logger.LogError(ex, $"Exception while getting next process execution. Assuming transient error and retrying in {delaySeconds} seconds.");
					await Task.Delay(delaySeconds * 1000);
					attempt++;
				}
			}
			throw new InvalidOperationException(); // Should never get here.
		}

		protected virtual async Task EnqueueProcessExecutionAsync<TProcess>(string processId, DateTime? executeAfterUtc = null) where TProcess : IProcess
		{
			await _executionQueue.EnqueueExecutionAsync(Guid.NewGuid().ToString(), new ProcessIdentifier(typeof(TProcess).Name, processId), executeAfterUtc.GetValueOrDefault(DateTime.UtcNow));
			_enqueueSignal.Set();
		}

		protected virtual void RegisterProcess<TProcess>(TProcess process) where TProcess : IProcess
		{
			_processes.Add(typeof(TProcess).Name, process);
		}

		protected virtual bool IsHydrationCaughtUp(SubscriberEvent subscriberEvent)
		{
			var regionalEndOfSubscription = _regionalEndsOfSubscription[subscriberEvent.RegionId];
			// Reached the end of the regional subscription stream?
			if (!regionalEndOfSubscription.HasValue || subscriberEvent.SubscriptionPosition >= regionalEndOfSubscription.Value)
			{
				_regionalEndsReached.TryUpdate(subscriberEvent.RegionId, true, false);
			}

			// Is end of stream reached for all regional subscription streams?
			// (End of stream as of the moment the last positions were retrieved. Subscriptions may have accumulated more events since then.)
			return (!_regionalEndsReached.Any(x => x.Value == false));
		}

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
			if (!_caughtUpSignal.IsSet && IsHydrationCaughtUp(subscriberEvent))
			{
				_caughtUpSignal.Set();
			}

			// Does nothing if no handler - event is ignored.
			if (this.GetType().GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IHandleBusinessEvent<>) && x.GetGenericArguments()[0] == subscriberEvent.ResolvedEventType))
			{
				await (Task)this.GetType().InvokeMember("HandleBusinessEventAsync", BindingFlags.InvokeMethod, null, this, new object[] { subscriberEvent.StreamId, subscriberEvent.Position, subscriberEvent.ResolvedEvent, cancellationToken });
			}
		}
	}
}
