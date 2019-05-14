using CommandLine;
using EventCore.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EventCore.Samples.GYEventStore.Cli
{
	class Program
	{
		public static Task Main(string[] args)
		{
			Console.WriteLine();

			IConfiguration configuration = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				// .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
				.AddCommandLine(args)
				.Build();

			var serviceCollection = new ServiceCollection();
			ConfigureServices(serviceCollection, configuration);

			var serviceProvider = serviceCollection.BuildServiceProvider();

			var subscribeLogger = new StandardLogger(serviceProvider.GetRequiredService<ILogger<Actions.SubscribeAction>>());
			var eventStoreUri = configuration.GetConnectionString("EventStoreRegionX");

			Parser.Default.ParseArguments<Options.SubscribeOptions>(args)
				.WithParsed<Options.SubscribeOptions>(options => new Actions.SubscribeAction(options, subscribeLogger, eventStoreUri).RunAsync().Wait());

			((IDisposable)serviceProvider)?.Dispose(); // Force flush log messages to output.

			return Task.CompletedTask;
		}

		private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
		{
			services.AddLogging(configure => configure.AddConfiguration(configuration.GetSection("Logging")).AddConsole());
		}
	}
}
