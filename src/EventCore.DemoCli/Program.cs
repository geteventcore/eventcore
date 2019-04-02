using CommandLine;
using System;
using System.Threading.Tasks;

namespace EventCore.DemoCli
{
	public class Program
	{
		public static Task Main(string[] args)
		{
			Parser.Default.ParseArguments<Options.CommitOptions, Options.LoadOptions>(args)
				.WithParsed<Options.CommitOptions>(options => new Actions.CommitAction(options).Run())
				.WithParsed<Options.LoadOptions>(options => new Actions.LoadAction(options).Run());

			return Task.CompletedTask;
		}
	}
}
