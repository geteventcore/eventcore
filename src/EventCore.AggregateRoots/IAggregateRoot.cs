using System.Threading;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public interface IAggregateRoot
	{
		Task<IHandledCommandResult> HandleGenericCommandAsync<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : ICommand;
	}
}
