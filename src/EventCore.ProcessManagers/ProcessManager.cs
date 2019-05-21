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

		protected readonly HashSet<IProcess> _processes = new HashSet<IProcess>();
		protected readonly ManualResetEventSlim _isHydrationCaughtUpSignal = new ManualResetEventSlim(false);

		public ProcessManager(ProcessManagerDependencies dependencies)
		{
			_logger = dependencies.Logger;
			_subscriber = dependencies.SubscriberFactory.Create(dependencies.Logger, dependencies.StreamClientFactory, dependencies.StreamStateRepo, dependencies.EventResolver, this, this, dependencies.SubscriberFactoryOptions, dependencies.SubscriptionStreamIds);
			_stateRepo = dependencies.ProcessManagerStateRepo;
		}

		public virtual async Task RunAsync(CancellationToken cancellationToken)
		{
			await Task.WhenAll(new[] { _subscriber.SubscribeAsync(cancellationToken), ManageAsync(cancellationToken) });
		}

		protected virtual async Task ManageAsync(CancellationToken cancellationToken)
		{
			await Task.WhenAny(new[] { _isHydrationCaughtUpSignal.WaitHandle.AsTask(), cancellationToken.WaitHandle.AsTask() });

			while (!cancellationToken.IsCancellationRequested)
			{
				// Run processes in order of due date.
			}
		}

		protected virtual async Task EnqueueProcessAsync<TProcess>(string processId, DateTime? dueUtc) where TProcess : IProcess
		{
			await _stateRepo.AddOrUpdateQueuedProcessAsync(typeof(TProcess).Name, processId, dueUtc.GetValueOrDefault(DateTime.UtcNow));
		}

		protected virtual async Task TerminateProcessAsync<TProcess>(string processId) where TProcess : IProcess
		{
			await _stateRepo.RemoveQueuedProcessAsync(typeof(TProcess).Name, processId);
		}

		protected virtual async Task ExecuteProcessAsync(string processType, string processId)
		{
			var process = (IProcess)_processes.First(x => string.Equals(x.GetType().Name, processType, StringComparison.OrdinalIgnoreCase));
			await process.ExecuteAsync(processId);
		}

		protected virtual void RegisterProcess<TProcess>(TProcess process) where TProcess : IProcess
		{
			_processes.Add(process);
		}

		public virtual string SortSubscriberEventToParallelKey(SubscriberEvent subscriberEvent)
		{
			// Default is no parallel key, i.e. all events from the subscription stream
			// will be handled sequentially with no optimization to execute events in parallel.
			// The implementing class should override this if events can be consumed in parallel.
			return DEFAULT_PARALLEL_KEY; // Sorting keys may not be null or empty.
		}

		public virtual async Task HandleSubscriberEventAsync(SubscriberEvent subscriberEvent, CancellationToken cancellationToken)
		{
			// TODO: Check for caught-up condition and trigger signal...

			// Does nothing if no handler - event is ignored.
			if (this.GetType().GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IHandleBusinessEvent<>) && x.GetGenericArguments()[0] == subscriberEvent.ResolvedEventType))
			{
				await (Task)this.GetType().InvokeMember("HandleBusinessEventAsync", BindingFlags.InvokeMethod, null, this, new object[] { subscriberEvent.StreamId, subscriberEvent.Position, subscriberEvent.ResolvedEvent, cancellationToken });
			}
		}
	}
}
