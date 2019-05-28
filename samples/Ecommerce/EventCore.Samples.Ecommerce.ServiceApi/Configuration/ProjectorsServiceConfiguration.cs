using EventCore.EventSourcing;
using EventCore.Projectors;
using EventCore.Samples.Ecommerce.Domain;
using EventCore.Samples.Ecommerce.ServiceApi.Infrastructure;
using EventCore.Samples.Ecommerce.ServiceApi.Settings;
using EventCore.Samples.Ecommerce.Shared;
using EventCore.StatefulSubscriber;
using EventCore.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventCore.Samples.Ecommerce.ServiceApi.Configuration
{
	public static class ProjectorsServiceConfiguration
	{
		public static void Configure(IConfiguration config, IServiceCollection services)
		{
			services.AddSingleton<ISubscriberFactory, SubscriberFactory>();

			// EmailQueue
			AddDbContextScopeFactory<Projections.EmailReport.EmailReportDb.EmailReportDbContext>(services);
			services.AddDbContext<Projections.EmailReport.EmailReportDb.EmailReportDbContext>(o => o.UseSqlServer(ConnectionStrings.Get(config).ProjectionsDb));
			services.AddSingleton<Projections.EmailReport.EmailReportProjector>(sp => (Projections.EmailReport.EmailReportProjector)BuildProjector<Projections.EmailReport.EmailReportProjector>(sp, config));
			services.AddHostedService<ProjectorHostedService<Projections.EmailReport.EmailReportProjector>>();

			// SalesReport
			AddDbContextScopeFactory<Projections.SalesReport.SalesReportDb.SalesReportDbContext>(services);
			services.AddDbContext<Projections.SalesReport.SalesReportDb.SalesReportDbContext>(o => o.UseSqlServer(ConnectionStrings.Get(config).ProjectionsDb));
			services.AddSingleton<Projections.SalesReport.SalesReportProjector>(sp => (Projections.SalesReport.SalesReportProjector)BuildProjector<Projections.SalesReport.SalesReportProjector>(sp, config));
			services.AddHostedService<ProjectorHostedService<Projections.SalesReport.SalesReportProjector>>();
		}

		private static void AddDbContextScopeFactory<TContext>(IServiceCollection services)
			where TContext : DbContext
		{
			services.AddSingleton<IDbContextScopeFactory<TContext>, ProjectorDbContextScopeFactory<TContext>>();
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

			var baseDependencies = new ProjectorDependencies(logger, subscriberFactory, subFactoryOptions, streamClientFactory, streamStateRepo, resolver, MapSubscriptionStreams(sharedSettings.SubscriptionStreams));

			if (projectorType == typeof(Projections.EmailReport.EmailReportProjector))
				return new Projections.EmailReport.EmailReportProjector(baseDependencies, sp.GetRequiredService<IDbContextScopeFactory<Projections.EmailReport.EmailReportDb.EmailReportDbContext>>());

			if (projectorType == typeof(Projections.SalesReport.SalesReportProjector))
				return new Projections.SalesReport.SalesReportProjector(baseDependencies, sp.GetRequiredService<IDbContextScopeFactory<Projections.SalesReport.SalesReportDb.SalesReportDbContext>>());

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
