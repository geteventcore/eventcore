using EventCore.EventSourcing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots.EntityFrameworkState
{
	public class DbContextAggregateRootState<TContext> : AggregateRootState
		where TContext : DbContext, IStoreCausalIdHistory
	{
		private readonly TContext _db;

		public DbContextAggregateRootState(IBusinessEventResolver resolver, TContext db) : base(resolver)
		{
			_db = db;
		}

		public override async Task HydrateFromCheckpointAsync(Func<Func<StreamEvent, Task>, Task> streamLoaderAsync, CancellationToken cancellationToken)
		{
			// Do not catch exceptions - allow to bubble up to command handler.
			await streamLoaderAsync(se => ReceiveHydrationEventAsync(this, _resolver, _db, se, cancellationToken));
		}

		private static async Task<long> ReceiveHydrationEventAsync(DbContextAggregateRootState<TContext> state, IBusinessEventResolver resolver, TContext db, StreamEvent streamEvent, CancellationToken cancellationToken)
		{
			if (resolver.CanResolve(streamEvent.EventType))
			{
				var resolvedEvent = resolver.Resolve(streamEvent.EventType, streamEvent.Data);

				// Expecting that agg root stream does not have link events.
				await ApplyGenericBusinessEventAsync(state, streamEvent.StreamId, streamEvent.Position, resolvedEvent, cancellationToken);
			}

			// Track the position even if we can't resolve the event.
			return streamEvent.Position;
		}

		public override Task AddCausalIdToHistoryAsync(string causalId) => _db.AddCausalIdToHistoryIfNotExistsAsync(causalId);
		public override Task<bool> IsCausalIdInHistoryAsync(string causalId) => _db.ExistsCausalIdInHistoryAsync(causalId);
	}
}
