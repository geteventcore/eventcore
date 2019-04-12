using EventCore.EventSourcing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public interface ICommandHandlerFactory<TState> where TState : IAggregateRootState
	{
		ICommandHandler<TState, TCommand> Create<TCommand>() where TCommand : Command;
	}
}
