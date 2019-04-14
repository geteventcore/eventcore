using EventCore.EventSourcing;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain
{
	public interface IExecuteClientCommand<TCommand> where TCommand : DomainCommand
	{
		Task ExecuteAsync(TCommand command);
	}
}
