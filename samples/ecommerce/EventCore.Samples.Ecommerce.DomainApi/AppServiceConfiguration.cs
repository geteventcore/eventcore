using EventCore.Samples.Ecommerce.Domain;
using EventCore.Samples.Ecommerce.DomainApi.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace EventCore.Samples.Ecommerce.DomainApi
{
	public static class AppServiceConfiguration
	{
		public static void ConfigureServices(IConfiguration config, IServiceCollection services)
		{
			// Set up strongly typed configuration options.
			// services.Configure<EventSourcingOptions>(Configuration.GetSection("EventSourcing")); // Note: Nothing outside of startup needs access to event sourcing options.

			var options = config.GetSection("Infrastructure").Get<InfrastructureOptions>();

			// Set up basic app dependencies.
			services.AddSingleton<Utilities.IStandardLogger>(sp => new Utilities.StandardLogger(sp.GetRequiredService<ILogger<Startup>>()));
			services.AddScoped<EventSourcing.IStreamIdBuilder, EventSourcing.StreamIdBuilder>();
			ConfigureStreamClient(config, services, options);

			services.AddSingleton<EventSourcing.IBusinessEventResolver, AllBusinessEventsResolver>();

			AggregateRootsServiceConfiguration.ConfigureAggregateRoots(config, services, options);
		}


		private static void ConfigureStreamClient(IConfiguration config, IServiceCollection services, InfrastructureOptions options)
		{
			var connectionBuilders = new Dictionary<string, Func<EventStore.ClientAPI.IEventStoreConnection>>();

			// Connection factory must be able to create a new connection for each region.
			connectionBuilders.Add(Domain.Constants.DEFAULT_REGION_ID, () => EventStore.ClientAPI.EventStoreConnection.Create("EventStoreX"));

			services.AddScoped<EventSourcing.EventStore.IEventStoreConnectionFactory>(
				sp => new EventSourcing.EventStore.EventStoreConnectionFactory(connectionBuilders)
			);

			services.AddScoped<EventSourcing.IStreamClient>(
				sp => new EventSourcing.EventStore.EventStoreStreamClient(
					sp.GetRequiredService<Utilities.IStandardLogger>(),
					sp.GetRequiredService<EventSourcing.EventStore.IEventStoreConnectionFactory>(),
					new EventSourcing.EventStore.EventStoreStreamClientOptions(options.StreamReadBatchSize)
				)
			);
		}


	}
}
