using EventCore.EventSourcing;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public interface IAggregateRoot
	{
		Task<IHandledCommandResult> HandleGenericCommandAsync<TCommand>(TCommand command) where TCommand : Command;
	}
}
