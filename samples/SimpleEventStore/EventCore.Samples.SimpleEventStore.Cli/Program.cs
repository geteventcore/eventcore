using CommandLine;
using EventCore.Samples.SimpleEventStore.StreamDb;
using EventCore.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EventCore.Samples.SimpleEventStore.Cli
{
	class Program
	{
		public static Task Main(string[] args)
		{
			Console.WriteLine();

			var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

			IConfiguration configuration = new ConfigurationBuilder()
				.SetBasePath(Path.Combine(Directory.GetCurrentDirectory()))
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
				.AddCommandLine(args)
				.Build();

			var serviceCollection = new ServiceCollection();
			ConfigureServices(serviceCollection, configuration);

			var serviceProvider = serviceCollection.BuildServiceProvider();

			var subscribeLogger = new StandardLogger(serviceProvider.GetRequiredService<ILogger<Actions.SubscribeAction>>());
			var notificationsHubUrl = configuration.GetValue<string>("NotificationsHubUrl");

			Parser.Default.ParseArguments<Options.SubscribeOptions>(args)
				.WithParsed<Options.SubscribeOptions>(options => new Actions.SubscribeAction(options, subscribeLogger, serviceProvider.GetRequiredService<IServiceScopeFactory>(), notificationsHubUrl).RunAsync().Wait());

			((IDisposable) serviceProvider)?.Dispose(); // Force flush log messages to output.

			return Task.CompletedTask;
		}

		private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
		{
			services.AddLogging(configure => configure.AddConfiguration(configuration.GetSection("Logging")).AddConsole());

			services.AddDbContext<StreamDbContext>(o => o.UseSqlServer(configuration.GetConnectionString("StreamDbRegionX")));
		}
	}
}
