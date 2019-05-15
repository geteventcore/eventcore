using EventCore.AggregateRoots;
using EventCore.EventSourcing;
using EventCore.Samples.Ecommerce.Domain;
using EventCore.Samples.Ecommerce.Domain.State;
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
			// ConfigureEmailBuilder(services, options, config);
			// ConfigureEmailQueue(services, options);
			ConfigureSalesOrder(services, options);
		}

		private static void ConfigureGenericAggregateRoot<TAggregate, TState>(IServiceCollection services, ServicesOptions options)
			where TAggregate : AggregateRoot<TState>
			where TState : IAggregateRootState
		{
			services.AddScoped<AggregateRootDependencies<TState>>(sp => new AggregateRootDependencies<TState>(
				sp.GetRequiredService<IStandardLogger<TAggregate>>(),
				sp.GetRequiredService<IAggregateRootStateRepo<TState>>(),
				sp.GetRequiredService<IStreamIdBuilder>(),
				sp.GetRequiredService<IStreamClientFactory>(),
				new AllBusinessEventsResolver(sp.GetRequiredService<IStandardLogger<TAggregate>>()) // This resolver used when committing events.
			));

			// This resolver used when hydrating aggregate root state.
			services.AddSingleton<AggregateRootStateBusinessEventResolver<TState>>(sp => new AggregateRootStateBusinessEventResolver<TState>(
				sp.GetRequiredService<IStandardLogger<TAggregate>>()
			));

			services.AddScoped<TAggregate>();
		}

		private static void ConfigureGenericSerializableState<TState>(IServiceCollection services, Func<IServiceProvider, string, TState> stateConstructor, ServicesOptions options)
			where TState : ISerializableAggregateRootState
		{
			services.AddScoped<IAggregateRootStateRepo<TState>>(
				sp => new FlatFileAggregateRootStateRepo<TState>(
					sp.GetRequiredService<IStreamClientFactory>(),
					(regionId) => stateConstructor(sp, regionId), options.AggregateRootStateBasePath
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

			services.AddScoped<Domain.EmailBuilder.EmailBuilderStateFactory>(
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

			services.AddScoped<IAggregateRootStateRepo<Domain.EmailBuilder.EmailBuilderState>>(
				sp => new Domain.EmailBuilder.EmailBuilderStateRepo(
					sp.GetRequiredService<IStreamClientFactory>(),
					sp.GetRequiredService<Domain.EmailBuilder.EmailBuilderStateFactory>()
				)
			);
		}

		private static void ConfigureEmailQueue(IServiceCollection services, ServicesOptions options)
		{
			ConfigureGenericAggregateRoot<Domain.EmailQueue.EmailQueueAggregate, Domain.EmailQueue.EmailQueueState>(services, options);

			ConfigureGenericSerializableState<Domain.EmailQueue.EmailQueueState>(
				services,
				(sp, regionId) => new Domain.EmailQueue.EmailQueueState(sp.GetRequiredService<AggregateRootStateBusinessEventResolver<Domain.EmailQueue.EmailQueueState>>()),
				options
			);
		}

		private static void ConfigureSalesOrder(IServiceCollection services, ServicesOptions options)
		{
			ConfigureGenericAggregateRoot<Domain.SalesOrder.SalesOrderAggregate, Domain.SalesOrder.SalesOrderState>(services, options);

			ConfigureGenericSerializableState<Domain.SalesOrder.SalesOrderState>(
				services,
				(sp, regionId) => new Domain.SalesOrder.SalesOrderState(sp.GetRequiredService<AggregateRootStateBusinessEventResolver<Domain.SalesOrder.SalesOrderState>>()),
				options
			);
		}
	}
}
