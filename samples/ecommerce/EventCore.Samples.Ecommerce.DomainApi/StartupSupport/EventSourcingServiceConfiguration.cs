using EventCore.Samples.Ecommerce.DomainApi.Options;
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
			var connectionBuilders = new Dictionary<string, Func<EventStore.ClientAPI.IEventStoreConnection>>();

			// Connection factory must be able to create a new connection for each region.
			// ... For now we only have one region.
			connectionBuilders.Add(Domain.Constants.DEFAULT_REGION_ID, () => EventStore.ClientAPI.EventStoreConnection.Create("EventStoreX"));

			services.AddSingleton<EventSourcing.EventStore.IEventStoreConnectionFactory>(
				sp => new EventSourcing.EventStore.EventStoreConnectionFactory(connectionBuilders)
			);
			
			services.AddSingleton<EventSourcing.IStreamIdBuilder, EventSourcing.StreamIdBuilder>();
		}

		public static EventSourcing.IStreamClient BuildStreamClient<TLoggerCategory>(IServiceProvider sp, ServicesOptions options)
			=> new EventSourcing.EventStore.EventStoreStreamClient(
					sp.GetRequiredService<Utilities.IStandardLogger<TLoggerCategory>>(),
					sp.GetRequiredService<EventSourcing.EventStore.IEventStoreConnectionFactory>(),
					new EventSourcing.EventStore.EventStoreStreamClientOptions(options.StreamReadBatchSize)
				);
	}
}
