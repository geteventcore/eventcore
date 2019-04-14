using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public interface ICommandHandler<TState, TCommand>
		where TState : IAggregateRootState
		where TCommand : Command
	{
		Task<ICommandValidationResult> ValidateForStateAsync(TState state, TCommand c);
		Task<ICommandEventsResult> ProcessCommandAsync(TState state, TCommand c);
	}
}
