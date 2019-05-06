using EventCore.Samples.Ecommerce.DomainApi.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventCore.Samples.Ecommerce.DomainApi.StartupSupport
{
	public static class ServiceConfiguration
	{
		public static void Configure(IConfiguration config, IServiceCollection services)
		{
			var options = config.GetSection("Services").Get<ServicesOptions>();

			services.AddSingleton(typeof(Utilities.IStandardLogger<>), typeof(Utilities.StandardLogger<>));
			
			EventSourcingServiceConfiguration.Configure(config, services, options);
			AggregateRootsServiceConfiguration.Configure(config, services, options);
		}

	}
}
