using EventCore.AggregateRoots;
using EventCore.AggregateRoots.SerializableState;
using EventCore.Samples.EmailSystem.Domain.EmailBuilder.StateModels;
using EventCore.Samples.EmailSystem.DomainApi.Infrastructure;
using EventCore.Samples.EmailSystem.DomainApi.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EventCore.Samples.EmailSystem.DomainApi
{
	public static class AggregateRootsServiceConfiguration
	{
		public static void ConfigureAggregateRoots(IConfiguration config, IServiceCollection services, EventSourcingOptions eventSourcingOptions)
		{
			ConfigureSerializableStateRepo(services);

			ConfigureGenericAggregateRoot<Domain.EmailBuilder.EmailBuilderAggregate, Domain.EmailBuilder.EmailBuilderState>(services);
			ConfigureGenericAggregateRoot<Domain.EmailQueue.EmailQueueAggregate, Domain.EmailQueue.EmailQueueState>(services);
			ConfigureGenericAggregateRoot<Domain.SalesOrder.SalesOrderAggregate, Domain.SalesOrder.SalesOrderState>(services);

			ConfigureEmailBuilder(config, services);
			ConfigureEmailQueue(services);
		}

		private static void ConfigureSerializableStateRepo(IServiceCollection services)
		{
			services.AddScoped<ISerializableAggregateRootStateObjectRepo, FileSerializableAggregateRootStateObjectRepo>();
		}

		private static void ConfigureGenericAggregateRoot<TAggregate, TState>(IServiceCollection services)
			where TAggregate : AggregateRoot<TState>
			where TState : IAggregateRootState
		{
			services.AddScoped<TAggregate>();
			services.AddScoped<AggregateRootDependencies<TState>>();
			services.AddScoped<AggregateRootStateBusinessEventResolver<TState>>();
		}

		private static void ConfigureSerializableState<TState, TInternalState>(
			IServiceCollection services,
			Func<AggregateRootStateBusinessEventResolver<TState>, ISerializableAggregateRootStateObjectRepo, string, string, string, string, TState> stateConstructor)
			where TState : SerializableAggregateRootState<TInternalState>
		{
			services.AddScoped<SerializableAggregateRootStateFactory<TState, TInternalState>>(
				sp => new SerializableAggregateRootStateFactory<TState, TInternalState>(
					sp.GetRequiredService<AggregateRootStateBusinessEventResolver<TState>>(),
					sp.GetRequiredService<ISerializableAggregateRootStateObjectRepo>(),
					(resolver, repo, regionId, context, aggregateRootName, aggregateRootId) =>
						stateConstructor((AggregateRootStateBusinessEventResolver<TState>)resolver, repo, regionId, context, aggregateRootName, aggregateRootId)
				)
			);
		}

		private static void ConfigureEmailBuilder(IConfiguration config, IServiceCollection services)
		{
			ConfigureGenericAggregateRoot<Domain.EmailBuilder.EmailBuilderAggregate, Domain.EmailBuilder.EmailBuilderState>(services);

			services.AddDbContext<RegionAEmailBuilderDbContext>(o => o.UseSqlite(config.GetConnectionString("EmailBuilderStateDbRegionA")));
			services.AddDbContext<RegionBEmailBuilderDbContext>(o => o.UseSqlite(config.GetConnectionString("EmailBuilderStateDbRegionB")));

			services.AddScoped<Domain.EmailBuilder.EmailBuilderStateFactory>(
				sp => new Domain.EmailBuilder.EmailBuilderStateFactory(
					sp.GetRequiredService<AggregateRootStateBusinessEventResolver<Domain.EmailBuilder.EmailBuilderState>>(),
					(regionId) =>
					{
						if (string.Equals(regionId, "RegionA", StringComparison.OrdinalIgnoreCase)) return sp.GetRequiredService<RegionAEmailBuilderDbContext>();
						if (string.Equals(regionId, "RegionB", StringComparison.OrdinalIgnoreCase)) return sp.GetRequiredService<RegionAEmailBuilderDbContext>();
						return null;
					}
				)
			);
		}

		// Cheap way to reuse database context object for different connection strings.
		private class RegionAEmailBuilderDbContext : Domain.EmailBuilder.StateModels.EmailBuilderDbContext
		{
			public RegionAEmailBuilderDbContext(DbContextOptions<EmailBuilderDbContext> options) : base(options)
			{
			}
		}
		private class RegionBEmailBuilderDbContext : Domain.EmailBuilder.StateModels.EmailBuilderDbContext
		{
			public RegionBEmailBuilderDbContext(DbContextOptions<EmailBuilderDbContext> options) : base(options)
			{
			}
		}

		private static void ConfigureEmailQueue(IServiceCollection services)
		{
			ConfigureSerializableState<Domain.EmailQueue.EmailQueueState, Domain.EmailQueue.StateModels.EmailQueueMessageModel>(
				services,
				(resolver, repo, regionId, context, aggregateRootName, aggregateRootId) =>
					new Domain.EmailQueue.EmailQueueState((AggregateRootStateBusinessEventResolver<Domain.EmailQueue.EmailQueueState>)resolver, repo, regionId, context, aggregateRootName, aggregateRootId)
			);
		}

		private static void ConfigureSalesOrder(IServiceCollection services)
		{
			ConfigureSerializableState<Domain.SalesOrder.SalesOrderState, Domain.SalesOrder.StateModels.SalesOrderModel>(
				services,
				(resolver, repo, regionId, context, aggregateRootName, aggregateRootId) =>
					new Domain.SalesOrder.SalesOrderState((AggregateRootStateBusinessEventResolver<Domain.SalesOrder.SalesOrderState>)resolver, repo, regionId, context, aggregateRootName, aggregateRootId)
			);
		}
	}
}
