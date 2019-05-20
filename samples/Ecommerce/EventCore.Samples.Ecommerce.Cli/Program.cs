using CommandLine;
using EventCore.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Cli
{
	class Program
	{
		public static Task Main(string[] args)
		{
			IConfiguration configuration = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddCommandLine(args)
				.Build();

			var serviceCollection = new ServiceCollection();
			ConfigureServices(serviceCollection, configuration);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			var logger = new StandardLogger(serviceProvider.GetRequiredService<ILogger<Program>>());

			Parser.Default.ParseArguments<Options.InitializeOptions, Options.ListenOptions, Options.ClearErrorsOptions>(args)
				.WithParsed<Options.InitializeOptions>(options => new Actions.InitializeAction(options, logger, configuration).RunAsync().Wait())
				.WithParsed<Options.ListenOptions>(options => new Actions.ListenAction(options, logger, configuration).RunAsync().Wait())
				.WithParsed<Options.ClearErrorsOptions>(options => new Actions.ClearErrorsAction(options, logger, configuration).RunAsync().Wait());

			((IDisposable)serviceProvider)?.Dispose(); // Force flush log messages to output.

			return Task.CompletedTask;
		}

		private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
		{
			services.AddLogging(configure => configure.AddConfiguration(configuration.GetSection("Logging")).AddConsole());
		}
	}
}
