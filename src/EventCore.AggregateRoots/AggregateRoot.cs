using EventCore.EventSourcing;
using EventCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public abstract class AggregateRoot<TState> : IAggregateRoot where TState : IAggregateRootState
	{
		private readonly IStandardLogger _logger;
		private readonly IAggregateRootStateFactory<TState> _stateFactory;
		private readonly IStreamIdBuilder _streamIdBuilder;
		private readonly IStreamClient _streamClient;
		private readonly IBusinessEventResolver _resolver;
		private readonly ICommandHandlerFactory<TState> _handlerFactory;
		private readonly ISerializedAggregateRootStateRepo _serializedAggregateRootStateRepo;

		private readonly string _context;
		private readonly string _aggregateRootName;

		public abstract bool SupportsSerializeableState { get; }

		public AggregateRoot(AggregateRootDependencies<TState> dependencies, string context, string aggregateRootName)
		{
			_logger = dependencies.Logger;
			_stateFactory = dependencies.StateFactory;
			_streamIdBuilder = dependencies.StreamIdBuilder;
			_streamClient = dependencies.StreamClient;
			_resolver = dependencies.Resolver;
			_handlerFactory = dependencies.HandlerFactory;
			_serializedAggregateRootStateRepo = dependencies.SerializedAggregateRootStateRepo;
			_context = context;
			_aggregateRootName = aggregateRootName;
		}

		public async Task<IHandledCommandResult> HandleGenericCommandAsync<TCommand>(TCommand command) where TCommand : ICommand
		{
			try
			{
				// Semantic validation to check format of fields, etc. - data that isn't dependent on state.
				var semanticValidationResult = command.ValidateSemantics();
				if (!semanticValidationResult.IsValid)
				{
					return HandledCommandResult.FromValidationErrors(semanticValidationResult.Errors.ToList());
				}

				var regionId = command.GetRegionId();
				var aggregateRootId = command.GetAggregateRootId();
				var streamId = _streamIdBuilder.Build(regionId, _context, _aggregateRootName, aggregateRootId);

				string serializedState = await TryLoadSerializeStateAsync(SupportsSerializeableState, _aggregateRootName, aggregateRootId, _serializedAggregateRootStateRepo, _logger);

				var state = _stateFactory.Create(serializedState);
				await state.HydrateAsync(_streamClient, streamId);

				// Check for duplicate command id.
				if (state.IsCausalIdInRecentHistory(command._Metadata.CommandId))
				{
					return HandledCommandResult.FromValidationError("Duplicate command id.");
				}

				var handler = _handlerFactory.Create<TCommand>();

				var validationResult = await handler.ValidateForStateAsync(state, command);
				if (!validationResult.IsValid)
				{
					return HandledCommandResult.FromValidationErrors(validationResult.Errors.ToList());
				}

				var eventsResult = await handler.ProcessCommandAsync(state, command);
				await ProcessEventsResultAsync(eventsResult, regionId, streamId, state, _resolver, _streamClient);

				await TrySaveSerializeStateAsync(state, SupportsSerializeableState, _aggregateRootName, aggregateRootId, _serializedAggregateRootStateRepo, _logger);

				return HandledCommandResult.FromSuccess();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception while handling generic command.");
				throw;
			}
		}

		public static async Task<string> TryLoadSerializeStateAsync(bool supportsSerializeableState, string aggregateRootName, string aggregateRootId, ISerializedAggregateRootStateRepo repo, IStandardLogger logger)
		{
			if (supportsSerializeableState)
			{
				try
				{
					return await repo.LoadStateAsync(aggregateRootName, aggregateRootId);
				}
				catch (Exception ex)
				{
					logger.LogWarning(ex, "Unable to load serialized state. This is a non-critical error.");
				}
			}
			return null;
		}

		public static async Task TrySaveSerializeStateAsync(TState state, bool supportsSerializeableState, string aggregateRootName, string aggregateRootId, ISerializedAggregateRootStateRepo repo, IStandardLogger logger)
		{
			if (supportsSerializeableState && state.SupportsSerialization)
			{
				try
				{
					var newSerializedState = await state.SerializeAsync();
					await repo.SaveStateAsync(aggregateRootName, aggregateRootId, newSerializedState);
				}
				catch (Exception ex)
				{
					logger.LogWarning(ex, "Unable to save serialized state. This is a non-critical error.");
				}
			}
		}

		public static async Task ProcessEventsResultAsync(ICommandEventsResult eventsResult, string regionId, string streamId, TState state, IBusinessEventResolver resolver, IStreamClient streamClient)
		{
			if (eventsResult.Events.Count > 0)
			{
				var commitEvents = new List<CommitEvent>();
				foreach (var businessEvent in eventsResult.Events)
				{
					if (!resolver.CanUnresolve(businessEvent))
					{
						throw new InvalidOperationException("Unable to unresolve business event.");
					}
					var unresolvedEvent = resolver.UnresolveEvent(businessEvent);
					commitEvents.Add(new CommitEvent(unresolvedEvent.EventType, unresolvedEvent.Data));
				}

				await streamClient.CommitEventsToStreamAsync(regionId, streamId, state.StreamPositionCheckpoint + 1, commitEvents);
			}
		}
	}
}
