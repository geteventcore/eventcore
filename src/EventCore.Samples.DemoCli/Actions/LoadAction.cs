using System;
using System.Threading.Tasks;

namespace EventCore.Samples.DemoCli.Actions
{
	public class LoadAction : IAction
	{
		public LoadAction(Options.LoadOptions options)
		{
		}

		public Task RunAsync()
		{
			Console.WriteLine("Running load...");
			return Task.CompletedTask;
		}
	}
}
