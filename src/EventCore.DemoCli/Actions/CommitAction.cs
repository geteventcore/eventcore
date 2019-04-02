using System;
using System.Threading.Tasks;

namespace EventCore.DemoCli.Actions
{
	public class CommitAction : IAction
	{
		public CommitAction(Options.CommitOptions options)
		{
		}

		public void Run()
		{
			Console.WriteLine("Running demo...");
		}
	}
}
