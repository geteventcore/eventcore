using EventCore.ProcessManagers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.ProcessManagers.EmailManagement.Processes
{
	public class EmailSendProcess : IProcess
	{
		public Task ExecuteAsync(string processId, CancellationToken cancellationToken)
		{
			Console.WriteLine($"EmailSendProcess: {processId}");
			return Task.CompletedTask;
		}
	}
}
