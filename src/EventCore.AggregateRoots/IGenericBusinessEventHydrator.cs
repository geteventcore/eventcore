using EventCore.EventSourcing;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public interface IGenericBusinessEventHydrator
	{
		Task ApplyGenericBusinessEventAsync(IAggregateRootState state, string streamId, long position, IBusinessEvent e, CancellationToken cancellationToken);
	}
}
