using EventCore.EventSourcing;
using EventCore.Projectors;
using EventCore.Samples.Ecommerce.Domain;
using EventCore.Samples.EventStore.StreamClient;
using EventCore.StatefulSubscriber;
using EventCore.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
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
					config.AddEnvironmentVariables();
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

			services.AddSingleton(typeof(Utilities.IStandardLogger<>), typeof(Utilities.StandardLogger<>));

			ConfigureEventSourcing(services, config);
			ConfigureHostedServices(services, config);
		}

		public static void ConfigureEventSourcing(IServiceCollection services, IConfiguration config)
		{
			var eventStoreRegionXUri = config.GetConnectionString("EventStoreRegionX");
			var eventStoreUris = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { Shared.Constants.DEFAULT_REGION_ID, eventStoreRegionXUri } };

			services.AddSingleton<IStreamClientFactory>(
				sp => new EventStoreStreamClientFactory(
					sp.GetRequiredService<IStandardLogger<EventStoreStreamClientFactory>>(),
					eventStoreUris,
					config.GetValue<int>("EventSourcing:StreamReadBatchSize"),
					5)
				);
		}

		public static void ConfigureHostedServices(IServiceCollection services, IConfiguration config)
		{
			services.AddSingleton<ISubscriberFactory, SubscriberFactory>();

			ConfigureProjectorSettings<Projections.EmailQueue.EmailQueueProjector>(services, config);
			services.AddSingleton<Projections.EmailQueue.EmailQueueProjector>(sp => (Projections.EmailQueue.EmailQueueProjector)BuildProjector<Projections.EmailQueue.EmailQueueProjector>(sp, config));
			services.AddHostedService<ProjectorHostedService<Projections.EmailQueue.EmailQueueProjector>>();

			ConfigureProjectorSettings<Projections.SalesReport.SalesReportProjector>(services, config);
			services.AddSingleton<Projections.SalesReport.SalesReportProjector>(sp => (Projections.SalesReport.SalesReportProjector)BuildProjector<Projections.SalesReport.SalesReportProjector>(sp, config));
			services.AddHostedService<ProjectorHostedService<Projections.SalesReport.SalesReportProjector>>();
		}

		public static void ConfigureProjectorSettings<TProjector>(IServiceCollection services, IConfiguration config) where TProjector : IProjector
		{
			services.Configure<ProjectorSettings>(typeof(TProjector).Name, config.GetSection($"Projectors:{typeof(TProjector).Name}"));
		}

		public static IProjector BuildProjector<TProjector>(IServiceProvider sp, IConfiguration config)
			where TProjector : IProjector
		{
			var settings = sp.GetRequiredService<IOptionsSnapshot<ProjectorSettings>>().Get(typeof(TProjector).Name);
			var subFactoryOptions = GetSubscriberFactoryOptions(settings);

			var logger = sp.GetRequiredService<IStandardLogger<TProjector>>();
			var subscriberFactory = sp.GetRequiredService<ISubscriberFactory>();
			var streamClientFactory = sp.GetRequiredService<IStreamClientFactory>();
			var streamStateRepo = new StreamStateRepo(logger, settings.StreamStateBasePath);
			var resolver = new AllBusinessEventsResolver(logger);

			var projectorType = typeof(TProjector);

			if (projectorType == typeof(Projections.EmailQueue.EmailQueueProjector))
				return new Projections.EmailQueue.EmailQueueProjector(logger, subscriberFactory, streamClientFactory, streamStateRepo, resolver, subFactoryOptions, MapSubscriptionStreams(settings.SubscriptionStreams));

			if (projectorType == typeof(Projections.SalesReport.SalesReportProjector))
				return new Projections.SalesReport.SalesReportProjector(logger, subscriberFactory, streamClientFactory, streamStateRepo, resolver, subFactoryOptions, MapSubscriptionStreams(settings.SubscriptionStreams));

			return null;
		}

		public static IList<SubscriptionStreamId> MapSubscriptionStreams(ProjectorSettings.SubscriptionStream[] streams) =>
			streams.Select(x => new SubscriptionStreamId(x.RegionId, x.StreamId)).ToList();

		public static SubscriberFactoryOptions GetSubscriberFactoryOptions(ProjectorSettings settings) =>
			new SubscriberFactoryOptions(
				settings.MaxResolutionQueueSize, settings.MaxSortingQueueSize,
				settings.MaxHandlingQueuesSharedSize, settings.MaxParallelHandlerExecutions
			);
	}
}
