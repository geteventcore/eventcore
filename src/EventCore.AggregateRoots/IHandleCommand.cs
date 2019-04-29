using System.Threading;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public interface IHandleCommand<TState, TCommand>
		where TState : IAggregateRootState
		where TCommand : ICommand
	{
		Task<ICommandResult> HandleCommandAsync(TState s, TCommand c, CancellationToken ct);
	}
}
