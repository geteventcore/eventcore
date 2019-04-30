using EventCore.AggregateRoots;
using EventCore.AggregateRoots.SerializableState;
using EventCore.EventSourcing;
using EventCore.Samples.Ecommerce.Domain;
using EventCore.Samples.Ecommerce.DomainApi.Options;
using EventCore.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EventCore.Samples.Ecommerce.DomainApi.StartupSupport
{
	public static class AggregateRootsServiceConfiguration
	{
		public static void Configure(IConfiguration config, IServiceCollection services, ServicesOptions options)
		{
			services.AddSingleton<ISerializableAggregateRootStateObjectRepo>(
				sp => new FileSerializableAggregateRootStateObjectRepo(options.AggregateRootStateBasePath)
			);

			// ConfigureEmailBuilder(services, options, config);
			// ConfigureEmailQueue(services, options);
			ConfigureSalesOrder(services, options);
		}

		private static void ConfigureGenericAggregateRoot<TAggregate, TState>(IServiceCollection services, ServicesOptions options)
			where TAggregate : AggregateRoot<TState>
			where TState : IAggregateRootState
		{
			services.AddSingleton<AggregateRootDependencies<TState>>(sp => new AggregateRootDependencies<TState>(
				sp.GetRequiredService<IStandardLogger<TAggregate>>(),
				sp.GetRequiredService<IAggregateRootStateFactory<TState>>(),
				sp.GetRequiredService<IStreamIdBuilder>(),
				EventSourcingServiceConfiguration.BuildStreamClient<TAggregate>(sp, options),
				new AllBusinessEventsResolver(sp.GetRequiredService<IStandardLogger<TAggregate>>()) // This resolver used when committing events.
			));

			services.AddSingleton<AggregateRootStateBusinessEventResolver<TState>>(sp => new AggregateRootStateBusinessEventResolver<TState>(
				sp.GetRequiredService<IStandardLogger<TAggregate>>()
			));

			services.AddScoped<TAggregate>();
		}

		private static void ConfigureGenericSerializableState<TState, TInternalState>(
			IServiceCollection services,
			Func<AggregateRootStateBusinessEventResolver<TState>, ISerializableAggregateRootStateObjectRepo, TState> stateConstructor)
			where TState : SerializableAggregateRootState<TInternalState>
		{
			services.AddSingleton<IAggregateRootStateFactory<TState>>(
				sp => new SerializableAggregateRootStateFactory<TState, TInternalState>(
					sp.GetRequiredService<AggregateRootStateBusinessEventResolver<TState>>(),
					sp.GetRequiredService<ISerializableAggregateRootStateObjectRepo>(),
					(resolver, repo) => stateConstructor((AggregateRootStateBusinessEventResolver<TState>)resolver, repo)
				)
			);
		}

		// ***
		// Everything below this line configures specific aggregate roots.
		// ***

		private static void ConfigureEmailBuilder(IServiceCollection services, ServicesOptions options, IConfiguration config)
		{
			ConfigureGenericAggregateRoot<Domain.EmailBuilder.EmailBuilderAggregate, Domain.EmailBuilder.EmailBuilderState>(services, options);

			services.AddDbContext<Domain.EmailBuilder.StateModels.EmailBuilderDbContext>(o => o.UseSqlite(config.GetConnectionString("EmailBuilderStateDb")));

			services.AddSingleton<Domain.EmailBuilder.EmailBuilderStateFactory>(
				sp => new Domain.EmailBuilder.EmailBuilderStateFactory(
					sp.GetRequiredService<AggregateRootStateBusinessEventResolver<Domain.EmailBuilder.EmailBuilderState>>(),
					(regionId) =>
					{
						// If supporting multiple regions then return db context based on region id given.
						// if (string.Equals(regionId, "RegionA", StringComparison.OrdinalIgnoreCase)) return sp.GetRequiredService<RegionAEmailBuilderDbContext>();
						// if (string.Equals(regionId, "RegionB", StringComparison.OrdinalIgnoreCase)) return sp.GetRequiredService<RegionBEmailBuilderDbContext>();
						// return null;
						return sp.GetRequiredService<Domain.EmailBuilder.StateModels.EmailBuilderDbContext>();
					}
				)
			);
		}

		private static void ConfigureEmailQueue(IServiceCollection services, ServicesOptions options)
		{
			ConfigureGenericAggregateRoot<Domain.EmailQueue.EmailQueueAggregate, Domain.EmailQueue.EmailQueueState>(services, options);

			ConfigureGenericSerializableState<Domain.EmailQueue.EmailQueueState, Domain.EmailQueue.StateModels.EmailQueueMessageModel>(
				services,
				(resolver, repo) => new Domain.EmailQueue.EmailQueueState((AggregateRootStateBusinessEventResolver<Domain.EmailQueue.EmailQueueState>)resolver, repo)
			);
		}

		private static void ConfigureSalesOrder(IServiceCollection services, ServicesOptions options)
		{
			ConfigureGenericAggregateRoot<Domain.SalesOrder.SalesOrderAggregate, Domain.SalesOrder.SalesOrderState>(services, options);

			ConfigureGenericSerializableState<Domain.SalesOrder.SalesOrderState, Domain.SalesOrder.StateModels.SalesOrderModel>(
				services,
				(resolver, repo) => new Domain.SalesOrder.SalesOrderState((AggregateRootStateBusinessEventResolver<Domain.SalesOrder.SalesOrderState>)resolver, repo)
			);
		}
	}
}
