using EventCore.EventSourcing;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Projections
{
	public interface IHandleBusinessEvent<TEvent> where TEvent : IBusinessEvent
	{
		Task HandleBusinessEventAsync(string streamId, long position, TEvent e, CancellationToken cancellationToken);
	}
}
