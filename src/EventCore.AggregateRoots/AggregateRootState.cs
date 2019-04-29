using EventCore.EventSourcing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	// Basic aggregate root state implementation.
	public abstract class AggregateRootState : IAggregateRootState
	{
		protected readonly IBusinessEventResolver _resolver;
		protected readonly IAggregateRootStateHydrator _genericHydrator;

		public virtual long? StreamPositionCheckpoint { get; protected set; }

		public AggregateRootState(IBusinessEventResolver resolver, IAggregateRootStateHydrator genericHydrator)
		{
			_resolver = resolver;
			_genericHydrator = genericHydrator;
		}

		public virtual async Task HydrateFromCheckpointAsync(Func<Func<StreamEvent, Task>, Task> streamLoaderAsync, CancellationToken cancellationToken)
		{
			// Do not catch exceptions - allow to bubble up to command handler.
			await streamLoaderAsync(async se =>
			{
				if (_resolver.CanResolve(se.EventType))
				{
					var resolvedEvent = _resolver.Resolve(se.EventType, se.Data);

					// Expecting that agg root stream does not have link events.
					await _genericHydrator.ApplyGenericBusinessEventAsync(this, se.StreamId, se.Position, resolvedEvent, cancellationToken);
				}
				
				// Track the position even if we can't resolve the event.
				StreamPositionCheckpoint = se.Position;
			});
		}

		public abstract Task<bool> IsCausalIdInHistoryAsync(string causalId);
		public abstract Task AddCausalIdToHistoryAsync(string causalId);
	}
}
