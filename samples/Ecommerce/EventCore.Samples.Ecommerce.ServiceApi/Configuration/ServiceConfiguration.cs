using EventCore.Samples.Ecommerce.ServiceApi.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventCore.Samples.Ecommerce.ServiceApi.Configuration
{
	public static class ServiceConfiguration
	{
		public static void Configure(IConfiguration config, IServiceCollection services)
		{
			services.AddSingleton(typeof(Utilities.IStandardLogger<>), typeof(Utilities.StandardLogger<>));
			
			EventSourcingServiceConfiguration.Configure(config, services);
			AggregateRootsServiceConfiguration.Configure(config, services);
			ProjectorsServiceConfiguration.Configure(config, services);
		}

	}
}
