using System.Threading;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public interface IAggregateRoot
	{
		Task<ICommandResult> HandleGenericCommandAsync(ICommand command, CancellationToken cancellationToken);
	}
}
