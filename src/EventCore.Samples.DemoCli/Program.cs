using CommandLine;
using System;
using System.Threading.Tasks;

namespace EventCore.Samples.DemoCli
{
	public class Program
	{
		public static Task Main(string[] args)
		{
			Console.WriteLine();
			Console.WriteLine("Demo CLI expects EventStore to be running on default TCP port 1113 with default credentials.");
			Console.WriteLine();

			Parser.Default.ParseArguments<Options.CommitOptions, Options.LoadOptions>(args)
				.WithParsed<Options.CommitOptions>(options => new Actions.CommitAction(options).RunAsync().Wait())
				.WithParsed<Options.LoadOptions>(options => new Actions.LoadAction(options).RunAsync().Wait());

			return Task.CompletedTask;
		}
	}
}
