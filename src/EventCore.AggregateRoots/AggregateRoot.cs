using EventCore.EventSourcing;
using EventCore.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public abstract class AggregateRoot<TState> : IAggregateRoot
		where TState : IAggregateRootState
	{
		private readonly IStandardLogger _logger;
		private readonly IAggregateRootStateRepo<TState> _stateRepo;
		private readonly IStreamIdBuilder _streamIdBuilder;
		private readonly IStreamClientFactory _streamClientFactory;
		private readonly IBusinessEventResolver _resolver;

		private readonly string _context;
		private readonly string _aggregateRootName;

		public AggregateRoot(AggregateRootDependencies<TState> dependencies, string context, string aggregateRootName)
		{
			_logger = dependencies.Logger;
			_stateRepo = dependencies.StateRepo;
			_streamIdBuilder = dependencies.StreamIdBuilder;
			_streamClientFactory = dependencies.StreamClientFactory;
			_resolver = dependencies.EventResolver;
			_context = context;
			_aggregateRootName = aggregateRootName;
		}

		protected virtual async Task<ICommandResult> InvokeTypedHandlerAsync(TState state, ICommand command, CancellationToken cancellationToken)
		{
			// Expects IHandleCommand<> for the type of command given.
			return await (Task<ICommandResult>)this.GetType().InvokeMember("HandleCommandAsync", BindingFlags.InvokeMethod, null, this, new object[] { state, command, cancellationToken });
		}

		public virtual async Task<ICommandResult> HandleGenericCommandAsync(ICommand command, CancellationToken cancellationToken)
		{
			try
			{
				// Semantic validation to check format of fields, etc. - data that isn't dependent on state.
				var semanticValidationResult = command.ValidateSemantics();
				if (!semanticValidationResult.IsValid)
				{
					return CommandResult.FromErrors(semanticValidationResult.Errors.ToList());
				}

				var regionId = command.GetRegionId();
				var aggregateRootId = command.GetAggregateRootId();
				var streamId = _streamIdBuilder.Build(regionId, _context, _aggregateRootName, aggregateRootId);

				// Load the latest aggregate root instance.
				var state = await _stateRepo.LoadAsync(regionId, streamId, cancellationToken);

				// Check for duplicate command id.
				if (await state.IsCausalIdInHistoryAsync(command.GetCommandId()))
				{
					return CommandResult.FromError("Duplicate command id.");
				}

				var handlerResult = await InvokeTypedHandlerAsync(state, command, cancellationToken);

				if (!handlerResult.IsSuccess)
				{
					return handlerResult;
				}

				// Commit events to the agg root stream.
				var trackingStartCommit = DateTime.Now;
				if (!(await CommitEventsAsync(handlerResult.Events, regionId, streamId, state.StreamPositionCheckpoint)))
				{
					return CommandResult.FromError("Concurrency conflict while committing events.");
				}

				return handlerResult;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception while handling generic command.");
				throw;
			}
		}

		protected virtual async Task<bool> CommitEventsAsync(IImmutableList<IBusinessEvent> events, string regionId, string streamId, long? lastPositionHydrated)
		{
			if (events.Count > 0)
			{
				var commitEvents = new List<CommitEvent>();
				foreach (var businessEvent in events)
				{
					if (!_resolver.CanUnresolve(businessEvent))
					{
						throw new InvalidOperationException("Unable to unresolve business event.");
					}
					var unresolvedEvent = _resolver.Unresolve(businessEvent);
					commitEvents.Add(new CommitEvent(unresolvedEvent.EventType, unresolvedEvent.Data));
				}

				var client = _streamClientFactory.Create(regionId); // Assuming caller will take care of disposing if necessary.
				
				var result = await client.CommitEventsToStreamAsync(streamId, lastPositionHydrated, commitEvents);

				if (result != CommitResult.Success)
				{
					return false;
				}
			}

			return true;
		}
	}
}
