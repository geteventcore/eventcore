using EventCore.EventSourcing;
using EventCore.Samples.Ecommerce.DomainApi.Options;
using EventCore.Samples.EventStore.StreamClient;
using EventCore.Utilities;
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

			// Connection factory must be able to create a new connection for each region.
			// ... For now we only have one region.
			var eventStoreRegionXUri = config.GetConnectionString("EventStoreRegionX");
			var eventStoreUris = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { Shared.Constants.DEFAULT_REGION_ID, eventStoreRegionXUri } };

			services.AddSingleton<IStreamClientFactory>(
				sp => new EventStoreStreamClientFactory(
					sp.GetRequiredService<IStandardLogger<EventStoreStreamClientFactory>>(),
					eventStoreUris,
					config.GetValue<int>("Services:StreamReadBatchSize"),
					5)
				);
			services.AddSingleton<IStreamIdBuilder, EventSourcing.StreamIdBuilder>();
		}
	}
}
