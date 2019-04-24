using EventCore.Samples.EmailSystem.DomainApi.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace EventCore.Samples.EmailSystem.DomainApi
{
	public static class AppServiceConfiguration
	{
		public static void ConfigureServices(IConfiguration config, IServiceCollection services)
		{
			// Set up strongly typed configuration options.
			// services.Configure<EventSourcingOptions>(Configuration.GetSection("EventSourcing")); // Note: Nothing outside of startup needs access to event sourcing options.

			var eventSourcingOptions = config.GetSection("EventSourcing").Get<EventSourcingOptions>();

			// Set up basic app dependencies.
			services.AddScoped<Utilities.IStandardLogger, Utilities.StandardLogger>();
			services.AddScoped<EventSourcing.IStreamIdBuilder, EventSourcing.StreamIdBuilder>();
			ConfigureStreamClient(config, services, eventSourcingOptions);

			AggregateRootsServiceConfiguration.ConfigureAggregateRoots(config, services, eventSourcingOptions);
		}


		private static void ConfigureStreamClient(IConfiguration config, IServiceCollection services, EventSourcingOptions eventSourcingOptions)
		{
			var connectionBuilders = new Dictionary<string, Func<EventStore.ClientAPI.IEventStoreConnection>>();

			var connStrRegionA = config.GetConnectionString("EventStoreRegionA");
			var connStrRegionB = config.GetConnectionString("EventStoreRegionB");

			// Connection factory must be able to create a new connection for each region.
			connectionBuilders.Add("RegionA", () => EventStore.ClientAPI.EventStoreConnection.Create("RegionA"));
			connectionBuilders.Add("RegionB", () => EventStore.ClientAPI.EventStoreConnection.Create("RegionB"));

			services.AddScoped<EventSourcing.EventStore.IEventStoreConnectionFactory>(
				sp => new EventSourcing.EventStore.EventStoreConnectionFactory(connectionBuilders)
			);

			services.AddScoped<EventSourcing.IStreamClient>(
				sp => new EventSourcing.EventStore.EventStoreStreamClient(
					sp.GetRequiredService<Utilities.IStandardLogger>(),
					sp.GetRequiredService<EventSourcing.EventStore.IEventStoreConnectionFactory>(),
					new EventSourcing.EventStore.EventStoreStreamClientOptions(eventSourcingOptions.StreamReadBatchSize)
				)
			);
		}


	}
}
