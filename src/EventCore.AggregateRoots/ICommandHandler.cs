using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public interface ICommandHandler<TState, TCommand>
		where TState : IAggregateRootState
		where TCommand : ICommand
	{
		Task<ICommandValidationResult> ValidateStateAsync(TState state, TCommand c);
		Task<ICommandEventsResult> ProcessCommandAsync(TState state, TCommand c);
	}
}
