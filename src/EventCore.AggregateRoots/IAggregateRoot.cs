using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public interface IAggregateRoot
	{
		Task<IHandledCommandResult> HandleGenericCommandAsync<TCommand>(TCommand command) where TCommand : ICommand;
	}
}
