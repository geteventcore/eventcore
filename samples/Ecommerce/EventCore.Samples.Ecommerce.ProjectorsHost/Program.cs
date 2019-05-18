using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.ProjectorsHost
{
	class Program
	{
		static async Task Main(string[] args)
		{
			await new HostBuilder()
				.ConfigureAppConfiguration((hostingContext, config) =>
				{
					config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
					config.AddCommandLine(args);
				})
				.ConfigureServices((hostContext, services) =>
				{
					ConfigureServices(services, hostContext.Configuration);
				})
				.ConfigureLogging((hostingContext, logging) =>
				{
					logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
					logging.AddConsole();
				})
				.RunConsoleAsync();
		}

		public static void ConfigureServices(IServiceCollection services, IConfiguration config)
		{
			services.AddOptions();

			// services.Configure<AppConfig>(hostContext.Configuration.GetSection("AppConfig"));

			// services.AddSingleton<IHostedService, PrintTextToConsoleService>();
		}

		public static void ConfigureSubscriber(IServiceCollection services, IConfiguration config)
		{

		}
	}
}
