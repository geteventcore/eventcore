using EventCore.ProcessManagers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.ProcessManagers.EmailManagement
{
	public class EmailBuilderProcess : IProcess
	{
		public Task ExecuteAsync(string processId, CancellationToken cancellationToken)
		{
			Console.WriteLine($"EmailBuilderProcess: {processId}");
			return Task.CompletedTask;
		}
	}
}
