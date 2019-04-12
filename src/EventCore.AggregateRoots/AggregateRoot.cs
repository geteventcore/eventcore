using EventCore.EventSourcing;
using EventCore.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public class AggregateRoot<TState> where TState : IAggregateRootState
	{
		private readonly IStandardLogger _logger;
		private readonly IAggregateRootStateFactory<TState> _stateFactory;
		private readonly IStreamIdBuilder _streamIdBuilder;
		private readonly IStreamClient _streamClient;
		private readonly IBusinessEventResolver _resolver;
		private readonly ICommandHandlerFactory<TState> _handlerFactory;
		
		private readonly string _context;
		private readonly string _aggregateRootName;

		public AggregateRoot(AggregateRootDependencies<TState> dependencies, string context, string aggregateRootName)
		{
			_logger = dependencies.Logger;
			_stateFactory = dependencies.StateFactory;
			_streamIdBuilder = dependencies.StreamIdBuilder;
			_streamClient = dependencies.StreamClient;
			_resolver = dependencies.Resolver;
			_handlerFactory = dependencies.HandlerFactory;
			_context = context;
			_aggregateRootName = aggregateRootName;
		}

		protected async Task<IHandledCommandResult> HandleGenericCommandAsync<TCommand>(TCommand command, string serializedState = null) where TCommand : Command
		{
			try
			{
				var regionId = command.RegionId();
				var aggregateRootId = command.AggregateRootId();
				var streamId = _streamIdBuilder.Build(regionId, _context, _aggregateRootName, aggregateRootId);

				var state = _stateFactory.Create(serializedState);
				await state.HydrateAsync(_streamClient, streamId);

				var handler = _handlerFactory.Create<TCommand>();

				var validationResult = await handler.ValidateCommandAsync(state, command);
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
