using EventCore.ProcessManagers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.ProcessManagers.EmailBuildManagement.Processes
{
	public class SalesReceiptEmailBuilderProcess : IProcess
	{
		public Task ExecuteAsync(string processId, CancellationToken cancellationToken)
		{
			Console.WriteLine($"SalesReceiptEmailBuilderProcess: {processId}");
			return Task.CompletedTask;
		}
	}
}
