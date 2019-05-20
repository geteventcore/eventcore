using EventCore.EventSourcing;
using EventCore.Projectors;
using EventCore.Samples.Ecommerce.Domain;
using EventCore.Samples.Ecommerce.ServiceApi.Infrastructure;
using EventCore.Samples.Ecommerce.ServiceApi.Settings;
using EventCore.StatefulSubscriber;
using EventCore.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventCore.Samples.Ecommerce.ServiceApi.StartupSupport
{
	public static class ProjectorsServiceConfiguration
	{
		public static void Configure(IConfiguration config, IServiceCollection services)
		{
			services.AddSingleton<ISubscriberFactory, SubscriberFactory>();

			services.AddSingleton<Projections.EmailQueue.EmailQueueProjector>(sp => (Projections.EmailQueue.EmailQueueProjector)BuildProjector<Projections.EmailQueue.EmailQueueProjector>(sp, config));
			// services.AddHostedService<ProjectorHostedService<Projections.EmailQueue.EmailQueueProjector>>();

			services.AddSingleton<Projections.SalesReport.SalesReportProjector>(sp => (Projections.SalesReport.SalesReportProjector)BuildProjector<Projections.SalesReport.SalesReportProjector>(sp, config));
			// services.AddHostedService<ProjectorHostedService<Projections.SalesReport.SalesReportProjector>>();
		}

		private static IProjector BuildProjector<TProjector>(IServiceProvider sp, IConfiguration config)
			where TProjector : IProjector
		{
			var sharedSettings = ProjectorsSettings.Get(config);
			var projectorSettings = ProjectorSettings.Get(config, typeof(TProjector).Name);
			var subFactoryOptions = GetSubscriberFactoryOptions(sharedSettings);

			var logger = sp.GetRequiredService<IStandardLogger<TProjector>>();
			var subscriberFactory = sp.GetRequiredService<ISubscriberFactory>();
			var streamClientFactory = sp.GetRequiredService<IStreamClientFactory>();
			var streamStateRepo = new StreamStateRepo(logger, projectorSettings.StreamStateBasePath);
			var resolver = new AllBusinessEventsResolver(logger);

			var projectorType = typeof(TProjector);

			if (projectorType == typeof(Projections.EmailQueue.EmailQueueProjector))
				return new Projections.EmailQueue.EmailQueueProjector(logger, subscriberFactory, streamClientFactory, streamStateRepo, resolver, subFactoryOptions, MapSubscriptionStreams(sharedSettings.SubscriptionStreams));

			if (projectorType == typeof(Projections.SalesReport.SalesReportProjector))
				return new Projections.SalesReport.SalesReportProjector(logger, subscriberFactory, streamClientFactory, streamStateRepo, resolver, subFactoryOptions, MapSubscriptionStreams(sharedSettings.SubscriptionStreams));

			return null;
		}

		private static IList<SubscriptionStreamId> MapSubscriptionStreams(SubscriptionStreamSettings[] streams) =>
			streams.Select(x => new SubscriptionStreamId(x.RegionId, x.StreamId)).ToList();

		private static SubscriberFactoryOptions GetSubscriberFactoryOptions(ProjectorsSettings settings) =>
			new SubscriberFactoryOptions(
				settings.MaxResolutionQueueSize, settings.MaxSortingQueueSize,
				settings.MaxHandlingQueuesSharedSize, settings.MaxParallelHandlerExecutions
			);
	}
}
