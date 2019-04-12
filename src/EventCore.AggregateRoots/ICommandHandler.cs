using EventCore.EventSourcing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public interface ICommandHandler<TState, TCommand>
		where TState : IAggregateRootState
		where TCommand : Command
	{
		Task<ICommandValidationResult> ValidateCommandAsync(TState state, TCommand command);
		Task<ICommandEventsResult> ProcessCommandAsync(TState state, TCommand command);
	}
}
