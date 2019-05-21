using EventCore.ProcessManagers;
using System;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.ProcessManagers.EmailManagement
{
	public class EmailBuilderProcess : IProcess
	{
		public Task ExecuteAsync(string processId)
		{
			Console.WriteLine($"EmailBuilderProcess: {processId}");
			return Task.CompletedTask;
		}
	}
}
