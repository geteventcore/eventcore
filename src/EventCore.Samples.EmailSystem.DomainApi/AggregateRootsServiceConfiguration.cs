using EventCore.Samples.EmailSystem.DomainApi.Infrastructure;
using EventCore.Samples.EmailSystem.DomainApi.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace EventCore.Samples.EmailSystem.DomainApi
{
	public static class AggregateRootsServiceConfiguration
	{
		public static void ConfigureAggregateRoots(IConfiguration config, IServiceCollection services, IOptionsSnapshot<EventSourcingOptions> eventSourcingOptions)
		{
			ConfigureSerializableStateRepo(services, eventSourcingOptions);

			ConfigureGenericAggregateRoot<Domain.EmailBuilder.EmailBuilderAggregate, Domain.EmailBuilder.EmailBuilderState>(services);
			ConfigureGenericAggregateRoot<Domain.EmailQueue.EmailQueueAggregate, Domain.EmailQueue.EmailQueueState>(services);
			ConfigureGenericAggregateRoot<Domain.SalesOrder.SalesOrderAggregate, Domain.SalesOrder.SalesOrderState>(services);

			ConfigureEmailBuilder(config, services);
			ConfigureEmailQueue(services);
		}

		private static void ConfigureSerializableStateRepo(IServiceCollection services, IOptionsSnapshot<EventSourcingOptions> eventSourcingOptions)
		{
			services.AddScoped<AggregateRoots.SerializableState.ISerializableAggregateRootStateObjectRepo, SerializableAggregateRootStateObjectRepo>();
		}

		private static void ConfigureGenericAggregateRoot<TAggregate, TState>(IServiceCollection services)
			where TAggregate : AggregateRoots.AggregateRoot<TState>
			where TState : AggregateRoots.IAggregateRootState
		{
			services.AddScoped<TAggregate>();
			services.AddScoped<AggregateRoots.AggregateRootDependencies<TState>>();
			services.AddScoped<AggregateRoots.AggregateRootStateBusinessEventResolver<TState>>();
		}

		private static void ConfigureEmailBuilder(IConfiguration config, IServiceCollection services)
		{
			ConfigureGenericAggregateRoot<Domain.EmailBuilder.EmailBuilderAggregate, Domain.EmailBuilder.EmailBuilderState>(services);

			services.AddDbContext<RegionAEmailBuilderDbContext>(o => o.UseSqlite(config.GetConnectionString("EmailBuilderStateDbRegionA")));
			services.AddDbContext<RegionBEmailBuilderDbContext>(o => o.UseSqlite(config.GetConnectionString("EmailBuilderStateDbRegionB")));

			services.AddScoped<Domain.EmailBuilder.EmailBuilderStateFactory>(
				sp => new Domain.EmailBuilder.EmailBuilderStateFactory(
					sp.GetRequiredService<AggregateRoots.AggregateRootStateBusinessEventResolver<Domain.EmailBuilder.EmailBuilderState>>(),
					(regionId) =>
					{
						if (string.Equals(regionId, "RegionA", StringComparison.OrdinalIgnoreCase)) return sp.GetRequiredService<RegionAEmailBuilderDbContext>();
						if (string.Equals(regionId, "RegionB", StringComparison.OrdinalIgnoreCase)) return sp.GetRequiredService<RegionAEmailBuilderDbContext>();
						return null;
					}
				)
			);
		}

		private class RegionAEmailBuilderDbContext : Domain.EmailBuilder.StateModels.EmailBuilderDbContext { }
		private class RegionBEmailBuilderDbContext : Domain.EmailBuilder.StateModels.EmailBuilderDbContext { }

		private static void ConfigureEmailQueue(IServiceCollection services)
		{
			services.AddScoped<AggregateRoots.SerializableState.SerializableAggregateRootStateFactory<Domain.EmailQueue.EmailQueueState, Domain.EmailQueue.StateModels.EmailQueueMessageModel>>(
				sp => new AggregateRoots.SerializableState.SerializableAggregateRootStateFactory<Domain.EmailQueue.EmailQueueState, Domain.EmailQueue.StateModels.EmailQueueMessageModel>(
					sp.GetRequiredService<AggregateRoots.AggregateRootStateBusinessEventResolver<Domain.EmailQueue.EmailQueueState>>(),
					sp.GetRequiredService<AggregateRoots.SerializableState.ISerializableAggregateRootStateObjectRepo>(),
					null
				)
			);
		}

		private static void ConfigureSerializableState<TState, TInternalState>(
			IServiceCollection services,
			Func<string, string, string, string, AggregateRoots.AggregateRootStateBusinessEventResolver<TState>, AggregateRoots.SerializableState.ISerializableAggregateRootStateObjectRepo, TState> stateConstructor)
			where TState : AggregateRoots.SerializableState.SerializableAggregateRootState<TInternalState>
		{
			services.AddScoped<AggregateRoots.SerializableState.SerializableAggregateRootStateFactory<TState, TInternalState>>(
				sp => new AggregateRoots.SerializableState.SerializableAggregateRootStateFactory<TState, TInternalState>(
					sp.GetRequiredService<AggregateRoots.AggregateRootStateBusinessEventResolver<TState>>(),
					sp.GetRequiredService<AggregateRoots.SerializableState.ISerializableAggregateRootStateObjectRepo>(),
					(regionId, context, aggregateRootName, aggregateRootId, resolver, repo) =>
						stateConstructor(
							regionId, context, aggregateRootName, aggregateRootId,
							(AggregateRoots.AggregateRootStateBusinessEventResolver<TState>)resolver,
							repo)
				)
			);
		}
	}
}
