using EventCore.EventSourcing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	// Basic aggregate root state implementation.
	public abstract class AggregateRootState : IAggregateRootState
	{
		private readonly IBusinessEventResolver _eventResolver;

		public virtual long? StreamPositionCheckpoint { get; protected set; }

		public AggregateRootState(IBusinessEventResolver eventResolver)
		{
			_eventResolver = eventResolver;
		}

		public abstract Task<bool> IsCausalIdInHistoryAsync(string causalId);

		protected abstract Task AddCausalIdToHistoryAsync(string causalId);

		public virtual async Task ApplyStreamEventAsync(StreamEvent streamEvent, CancellationToken cancellationToken)
		{
			if (_eventResolver.CanResolve(streamEvent.EventType))
			{
				var resolvedEvent = _eventResolver.Resolve(streamEvent.EventType, streamEvent.Data);

				// Expecting that agg root stream does not have link events.
				await ApplyGenericBusinessEventAsync(streamEvent.StreamId, streamEvent.Position, resolvedEvent, cancellationToken);

				var causalId = resolvedEvent.GetCausalId();
				if (!string.IsNullOrWhiteSpace(causalId))
				{
					await AddCausalIdToHistoryAsync(causalId);
				}
			}

			// Update the last hydrated position even if we can't resolve the event.
			StreamPositionCheckpoint = streamEvent.Position;
		}

		protected virtual async Task ApplyGenericBusinessEventAsync(string streamId, long position, IBusinessEvent e, CancellationToken cancellationToken)
		{
			// Expects IApplyBusinessEvent<TEvent> for the type of event given.
			await (Task)this.GetType().InvokeMember("ApplyBusinessEventAsync", BindingFlags.InvokeMethod, null, this, new object[] { streamId, position, e, cancellationToken });
		}
	}
}
