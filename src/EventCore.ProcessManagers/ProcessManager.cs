using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.ProcessManagers
{
	public abstract class ProcessManager : IProcessManager
	{
		protected readonly Dictionary<string, Type> _processTypesByName;
		protected readonly Dictionary<Type, string> _processTypesByType;

		protected readonly IProcessManagerStateRepo _stateRepo;

		public ProcessManager()
		{
			MapProcessTypes();
		}

		protected virtual void MapProcessTypes()
		{
			// Populate _processTypes for each IHaveProcess<TProcess>.
		}

		public virtual Task RunAsync(CancellationToken cancellationToken)
		{
			// Run hydration.

			// Wait for caught-up signal, then run manager.

			// Wait for both tasks to complete.

			throw new NotImplementedException();
		}

		protected virtual Task HydrateAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		protected virtual Task ManageAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		protected virtual async Task EnqueueProcessAsync<TProcess>(string processId, DateTime dueUtc) where TProcess : IProcess
		{
			await _stateRepo.AddOrUpdateProcessAsync(_processTypesByType[typeof(TProcess)], processId, dueUtc);
		}

		protected virtual async Task TerminateProcessAsync<TProcess>(string processId) where TProcess : IProcess
		{
			await _stateRepo.RemoveProcessAsync(_processTypesByType[typeof(TProcess)], processId);
		}

		protected virtual async Task ExecuteProcessAsync(string processType, string processId)
		{
			var type = _processTypesByName[processType];
			var method = this.GetType().GetMethod("CreateProcess");
			var generic = method.MakeGenericMethod(type);
			var process = (IProcess)generic.Invoke(this, null);
			await process.ExecuteAsync(processId);
		}
	}
}
