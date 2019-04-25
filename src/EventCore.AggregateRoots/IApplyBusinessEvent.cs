using EventCore.EventSourcing;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public interface IApplyBusinessEvent<TEvent> where TEvent : IBusinessEvent
	{
		Task ApplyBusinessEventAsync(string streamId, long position, TEvent e, CancellationToken cancellationToken);
	}
}
