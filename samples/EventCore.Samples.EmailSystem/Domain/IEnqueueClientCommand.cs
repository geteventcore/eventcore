using EventCore.EventSourcing;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain
{
	public interface IEnqueueClientCommand<TCommand> where TCommand : DomainCommand
	{
		Task EnqueueAsync(TCommand command);
	}
}
