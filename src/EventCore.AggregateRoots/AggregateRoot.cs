﻿using EventCore.EventSourcing;
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

		public async Task<IHandledCommandResult> HandleGenericCommandAsync<TCommand>(TCommand command) where TCommand : Command
		{
			try
			{
				// Semantic validation to check format of fields, etc. - data that isn't dependent on state.
				var semanticValidationResult = command.ValidateSemantics();
				if (!semanticValidationResult.IsValid)
				{
					return HandledCommandResult.FromValidationErrors(semanticValidationResult.Errors.ToList());
				}

				var regionId = command.RegionId();
				var aggregateRootId = command.AggregateRootId();
				var streamId = _streamIdBuilder.Build(regionId, _context, _aggregateRootName, aggregateRootId);

				string serializedState = null;
				if (SupportsSerializeableState)
				{
					try
					{
						serializedState = await _serializedAggregateRootStateRepo.LoadStateAsync(_aggregateRootName, aggregateRootId);
					}
					catch (Exception ex)
					{
						_logger.LogWarning(ex, "Unable to load serialized state. This is a non-critical error.");
					}
				}

				var state = _stateFactory.Create(serializedState);
				await state.HydrateAsync(_streamClient, streamId);

				// Check for duplicate command id.
				if (state.IsCausalIdInRecentHistory(command.Metadata.CommandId))
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
				if (eventsResult.Events.Count > 0)
				{
					var commitEvents = new List<CommitEvent>();
					foreach (var businessEvent in eventsResult.Events)
					{
						if (!_resolver.CanUnresolve(businessEvent))
						{
							throw new InvalidOperationException("Unable to unresolve business event.");
						}
						var unresolvedEvent = _resolver.UnresolveEvent(businessEvent);
						commitEvents.Add(new CommitEvent(unresolvedEvent.EventType, unresolvedEvent.Data));
					}

					await _streamClient.CommitEventsToStreamAsync(regionId, streamId, state.StreamPositionCheckpoint + 1, commitEvents);
				}

				if (SupportsSerializeableState && state.SupportsSerialization)
				{
					try
					{
						var newSerializedState = await state.SerializeAsync();
						await _serializedAggregateRootStateRepo.SaveStateAsync(_aggregateRootName, aggregateRootId, newSerializedState);
					}
					catch (Exception ex)
					{
						_logger.LogWarning(ex, "Unable to save serialized state. This is a non-critical error.");
					}
				}

				return HandledCommandResult.FromSuccess();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception while handling generic command.");
				throw;
			}
		}
	}
}
