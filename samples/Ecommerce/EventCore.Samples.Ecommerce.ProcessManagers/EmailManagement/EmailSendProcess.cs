using EventCore.ProcessManagers;
using System;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.ProcessManagers.EmailManagement
{
	public class EmailSendProcess : IProcess
	{
		public Task ExecuteAsync(string processId)
		{
			Console.WriteLine($"EmailSendProcess: {processId}");
			return Task.CompletedTask;
		}
	}
}
