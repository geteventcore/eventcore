using EventCore.EventSourcing;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public interface IAggregateRootState
	{
		long? StreamPositionCheckpoint { get; }
		Task HydrateAsync(IStreamClient streamClient, string streamId);
		Task ApplyGenericEventAsync(BusinessEvent e, CancellationToken cancellationToken);
	}
}
