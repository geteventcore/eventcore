﻿using EventCore.AggregateRoots;
using EventCore.EventSourcing;
using EventCore.Samples.Ecommerce.Domain.EmailBuilder.StateModels;
using EventCore.Samples.Ecommerce.Domain.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Domain.EmailBuilder
{
	public class EmailBuilderState : AggregateRootState,
		IApplyBusinessEvent<EmailEnqueuedEvent>
	{
		private readonly EmailBuilderDbContext _db;

		public EmailBuilderState(AggregateRootStateBusinessEventResolver<EmailBuilderState> resolver, EmailBuilderDbContext db) : base(resolver)
		{
			_db = db;
		}

		public override async Task HydrateFromCheckpointAsync(Func<Func<StreamEvent, Task>, Task> streamLoaderAsync, CancellationToken cancellationToken)
		{
			// Do not catch exceptions - allow to bubble up to command handler.
			await streamLoaderAsync(se => ReceiveHydrationEventAsync(se, cancellationToken));
		}

		public async Task<long> ReceiveHydrationEventAsync(StreamEvent streamEvent, CancellationToken cancellationToken)
		{
			if (_resolver.CanResolve(streamEvent.EventType))
			{
				var resolvedEvent = _resolver.Resolve(streamEvent.EventType, streamEvent.Data);

				// Expecting that agg root stream does not have link events, so we
				// pass the primary stream and position.
				await ApplyGenericBusinessEventAsync(streamEvent.StreamId, streamEvent.Position, resolvedEvent, cancellationToken);
			}

			// Track the position even if we can't resolve the event.
			return streamEvent.Position;
		}

		public override Task AddCausalIdToHistoryAsync(string causalId) => _db.AddCausalIdToHistoryIfNotExistsAsync(causalId);
		public override Task<bool> IsCausalIdInHistoryAsync(string causalId) => _db.ExistsCausalIdInHistoryAsync(causalId);

		public Task ApplyBusinessEventAsync(string streamId, long position, EmailEnqueuedEvent e, CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}