using CommandLine;
using System;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Cli
{
	class Program
	{
		public static Task Main(string[] args)
		{
			Console.WriteLine();

			Parser.Default.ParseArguments<Options.GenerateSalesOrdersOptions, Options.ListenSalesOrdersOptions>(args)
				.WithParsed<Options.GenerateSalesOrdersOptions>(options => new Actions.GenerateSalesOrdersAction(options).RunAsync().Wait())
				.WithParsed<Options.ListenSalesOrdersOptions>(options => new Actions.ListenSalesOrdersAction(options).RunAsync().Wait());

			return Task.CompletedTask;
		}
	}
}
