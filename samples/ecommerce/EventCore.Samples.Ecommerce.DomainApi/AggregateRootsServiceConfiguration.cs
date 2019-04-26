using EventCore.AggregateRoots;
using EventCore.AggregateRoots.SerializableState;
using EventCore.Samples.Ecommerce.Domain.EmailBuilder.StateModels;
using EventCore.Samples.Ecommerce.DomainApi.Infrastructure;
using EventCore.Samples.Ecommerce.DomainApi.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EventCore.Samples.Ecommerce.DomainApi
{
	public static class AggregateRootsServiceConfiguration
	{
		public static void ConfigureAggregateRoots(IConfiguration config, IServiceCollection services, InfrastructureOptions options)
		{
			ConfigureSupportingServices(services, options);

			ConfigureGenericAggregateRoot<Domain.EmailBuilder.EmailBuilderAggregate, Domain.EmailBuilder.EmailBuilderState>(services);
			ConfigureGenericAggregateRoot<Domain.EmailQueue.EmailQueueAggregate, Domain.EmailQueue.EmailQueueState>(services);
			ConfigureGenericAggregateRoot<Domain.SalesOrder.SalesOrderAggregate, Domain.SalesOrder.SalesOrderState>(services);

			ConfigureEmailBuilder(config, services);
			ConfigureEmailQueue(services);
		}

		private static void ConfigureSupportingServices(IServiceCollection services, InfrastructureOptions options)
		{
			services.AddScoped<IGenericBusinessEventHydrator, GenericBusinessEventHydrator>();
			services.AddScoped<ISerializableAggregateRootStateObjectRepo>(
				sp => new FileSerializableAggregateRootStateObjectRepo(options.AggregateRootStateBasePath)
			);
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
			Func<AggregateRootStateBusinessEventResolver<TState>, IGenericBusinessEventHydrator, ISerializableAggregateRootStateObjectRepo, string, string, string, string, TState> stateConstructor)
			where TState : SerializableAggregateRootState<TInternalState>
		{
			services.AddScoped<SerializableAggregateRootStateFactory<TState, TInternalState>>(
				sp => new SerializableAggregateRootStateFactory<TState, TInternalState>(
					sp.GetRequiredService<AggregateRootStateBusinessEventResolver<TState>>(),
					sp.GetRequiredService<IGenericBusinessEventHydrator>(),
					sp.GetRequiredService<ISerializableAggregateRootStateObjectRepo>(),
					(resolver, genericHydrator, repo, regionId, context, aggregateRootName, aggregateRootId) =>
						stateConstructor((AggregateRootStateBusinessEventResolver<TState>)resolver, genericHydrator, repo, regionId, context, aggregateRootName, aggregateRootId)
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
					sp.GetRequiredService<IGenericBusinessEventHydrator>(),
					(regionId) =>
					{
						if (string.Equals(regionId, "RegionA", StringComparison.OrdinalIgnoreCase)) return sp.GetRequiredService<RegionAEmailBuilderDbContext>();
						if (string.Equals(regionId, "RegionB", StringComparison.OrdinalIgnoreCase)) return sp.GetRequiredService<RegionBEmailBuilderDbContext>();
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
				(resolver, genericHydrator, repo, regionId, context, aggregateRootName, aggregateRootId) =>
					new Domain.EmailQueue.EmailQueueState(
						(AggregateRootStateBusinessEventResolver<Domain.EmailQueue.EmailQueueState>)resolver,
						genericHydrator, repo, regionId, context, aggregateRootName, aggregateRootId)
			);
		}

		private static void ConfigureSalesOrder(IServiceCollection services)
		{
			ConfigureSerializableState<Domain.SalesOrder.SalesOrderState, Domain.SalesOrder.StateModels.SalesOrderModel>(
				services,
				(resolver, genericHydrator, repo, regionId, context, aggregateRootName, aggregateRootId) =>
					new Domain.SalesOrder.SalesOrderState(
						(AggregateRootStateBusinessEventResolver<Domain.SalesOrder.SalesOrderState>)resolver,
						genericHydrator, repo, regionId, context, aggregateRootName, aggregateRootId)
			);
		}
	}
}
