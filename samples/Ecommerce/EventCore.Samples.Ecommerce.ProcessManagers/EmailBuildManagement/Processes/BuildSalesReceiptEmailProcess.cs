using EventCore.ProcessManagers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.ProcessManagers.EmailBuildManagement.Processes
{
	public class BuildSalesReceiptEmailProcess : IProcess
	{
		public Task ExecuteAsync(string correlationId, CancellationToken cancellationToken)
		{
			Console.WriteLine($"BuildSalesReceiptEmailProcess: {correlationId}");
			return Task.CompletedTask;
		}
	}
}
