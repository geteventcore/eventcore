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
		private readonly IAggregateRootStateFactory<TState> _stateFactory;
		private readonly IStreamIdBuilder _streamIdBuilder;
		private readonly IStreamClient _streamClient;
		private readonly IBusinessEventResolver _resolver;

		private readonly string _context;
		private readonly string _aggregateRootName;

		public AggregateRoot(AggregateRootDependencies<TState> dependencies, string context, string aggregateRootName)
		{
			_logger = dependencies.Logger;
			_stateFactory = dependencies.StateFactory;
			_streamIdBuilder = dependencies.StreamIdBuilder;
			_streamClient = dependencies.StreamClient;
			_resolver = dependencies.Resolver;
			_context = context;
			_aggregateRootName = aggregateRootName;
		}

		protected virtual async Task<ICommandResult> RouteCommandForTypedHandlingAsync(TState state, ICommand command, CancellationToken cancellationToken)
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

				// Instantiate the agg root state and load to latest checkpoint (i.e. cached state.)
				var state = await _stateFactory.CreateAndLoadToCheckpointAsync(regionId, _context, _aggregateRootName, aggregateRootId, cancellationToken);

				// Hydrate state from the latest checkpoint to the end of the agg root event stream.
				var lastPositionHydrated = await HydrateStateAsync(state, regionId, streamId, cancellationToken);

				// Check for duplicate command id.
				if (await state.IsCausalIdInHistoryAsync(command._Metadata.CommandId))
				{
					return CommandResult.FromError("Duplicate command id.");
				}

				// The agg root concrete implementation should take care of routing to typed command handlers for
				// each type of command it wants to handle.
				var handlerResult = await RouteCommandForTypedHandlingAsync(state, command, cancellationToken);

				if (!handlerResult.IsSuccess)
				{
					return handlerResult;
				}

				// Commit events to the agg root stream.
				if (!(await CommitEventsAsync(handlerResult.Events, regionId, streamId, lastPositionHydrated)))
				{
					return CommandResult.FromError("Concurrency conflict while committing events.");
				}

				// Save the command id to the causal id history so we can detect duplicate commands
				// as new commands arrive on this agg root instance.
				await state.AddCausalIdToHistoryAsync(command._Metadata.CommandId);

				return handlerResult;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception while handling generic command.");
				throw;
			}
		}

		protected virtual async Task<long?> HydrateStateAsync(TState state, string regionId, string streamId, CancellationToken cancellationToken)
		{
			var loadStreamFromPosition = state.StreamPositionCheckpoint.GetValueOrDefault(_streamClient.FirstPositionInStream - 1) + 1;
			long? lastPositionHydrated = null;

			await state.HydrateFromCheckpointAsync( // Must tell the state hydrator how to load events. I.e. give it a delegate.
				streamLoaderAsync => _streamClient.LoadStreamEventsAsync(
					regionId, streamId, loadStreamFromPosition,
					async se => // Function to execute for each event in the stream.
					{
						await streamLoaderAsync(se); // Calls the stream loader delegate given by the state hydration function.
						lastPositionHydrated = se.Position;
					},
					cancellationToken),
				cancellationToken
				);

			return lastPositionHydrated;
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

				var result = await _streamClient.CommitEventsToStreamAsync(regionId, streamId, lastPositionHydrated, commitEvents);

				if (result != CommitResult.Success)
				{
					return false;
				}
			}

			return true;
		}
	}
}
