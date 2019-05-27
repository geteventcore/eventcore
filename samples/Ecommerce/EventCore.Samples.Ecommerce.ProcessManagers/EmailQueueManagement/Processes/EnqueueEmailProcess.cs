using EventCore.ProcessManagers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.ProcessManagers.EmailQueueManagement.Processes
{
	public class EnqueueEmailProcess : IProcess
	{
		public Task ExecuteAsync(string correlationId, CancellationToken cancellationToken)
		{
			Console.WriteLine($"EnqueueEmailProcess: {correlationId}");
			return Task.CompletedTask;
		}
	}
}
