using System;
using System.Threading.Tasks;

namespace EventCore.DemoCli.Actions
{
	public class LoadAction : IAction
	{
		public LoadAction(Options.LoadOptions options)
		{
		}

		public void Run()
		{
			Console.WriteLine("Running fake...");
		}
	}
}
