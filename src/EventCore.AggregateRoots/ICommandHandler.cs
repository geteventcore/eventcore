using EventCore.EventSourcing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public interface ICommandHandler<TState, TCommand>
		where TState : AggregateRootState
		where TCommand : Command
	{
		Task<IHandledCommandResult> HandleAsync(TState state, TCommand command);
	}
}
