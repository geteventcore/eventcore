using EventCore.EventSourcing;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public interface IAggregateRootState
	{
		bool SupportsSerialization {get;}
		long? StreamPositionCheckpoint { get; }
		Task HydrateAsync(IStreamClient streamClient, string streamId);
		Task ApplyGenericBusinessEventAsync<TEvent>(TEvent e, CancellationToken cancellationToken) where TEvent : BusinessEvent;
		Task<string> SerializeAsync();
		bool IsCausalIdInRecentHistory(string causalId);
	}
}
