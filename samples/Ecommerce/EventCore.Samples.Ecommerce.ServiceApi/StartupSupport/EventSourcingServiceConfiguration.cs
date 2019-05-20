using EventCore.EventSourcing;
using EventCore.Samples.Ecommerce.ServiceApi.Settings;
using EventCore.Samples.EventStore.StreamClient;
using EventCore.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace EventCore.Samples.Ecommerce.ServiceApi.StartupSupport
{
	public static class EventSourcingServiceConfiguration
	{
		public static void Configure(IConfiguration config, IServiceCollection services)
		{

			// Connection factory must be able to create a new connection for each region.
			// ... For now we only have one region.
			var eventStoreRegionXUri = ConnectionStrings.Get(config).EventStoreRegionX;
			var eventStoreUris = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { Shared.Constants.DEFAULT_REGION_ID, eventStoreRegionXUri } };

			var settings = EventSourcingSettings.Get(config);

			services.AddSingleton<IStreamClientFactory>(
				sp =>
				{
					return new EventStoreStreamClientFactory(
						sp.GetRequiredService<IStandardLogger<EventStoreStreamClientFactory>>(),
						eventStoreUris, settings.StreamReadBatchSize, settings.ReconnectDelaySeconds);
				}
			);
			services.AddSingleton<IStreamIdBuilder, EventSourcing.StreamIdBuilder>();
		}
	}
}
