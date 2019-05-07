using EventCore.EventSourcing;
using EventCore.Samples.Ecommerce.DomainApi.Options;
using EventCore.Samples.EventStore.Client;
using EventCore.Samples.EventStore.StreamDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace EventCore.Samples.Ecommerce.DomainApi.StartupSupport
{
	public static class EventSourcingServiceConfiguration
	{
		public static void Configure(IConfiguration config, IServiceCollection services, ServicesOptions options)
		{
			var connectionBuilders = new Dictionary<string, Func<StreamDbContext>>();

			// Connection factory must be able to create a new connection for each region.
			// ... For now we only have one region.
			services.AddDbContext<StreamDbContext>(o => o.UseSqlite(config.GetConnectionString("EventStoreDbRegionX")));

			services.AddScoped<IDictionary<string, Func<StreamDbContext>>>(sp => new Dictionary<string, Func<StreamDbContext>>()
			{
				{ Domain.Constants.DEFAULT_REGION_ID, () => sp.GetRequiredService<StreamDbContext>() }
			});

			services.AddSingleton<IStreamIdBuilder, EventSourcing.StreamIdBuilder>();
		}

		public static IStreamClientFactory BuildStreamClientFactory<TLoggerCategory>(IServiceProvider sp, ServicesOptions options)
			=> new StreamClientFactory(
					sp.GetRequiredService<Utilities.IStandardLogger<TLoggerCategory>>(),
					sp.GetRequiredService<IDictionary<string, Func<StreamDbContext>>>(),
					options.EventStoreNotificationsHubUrl
				);
	}
}
