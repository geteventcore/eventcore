using EventCore.EventSourcing;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public interface IApplyBusinessEvent<TEvent> where TEvent : BusinessEvent
	{
		Task ApplyBusinessEventAsync(TEvent e, CancellationToken cancellationToken);
	}
}
