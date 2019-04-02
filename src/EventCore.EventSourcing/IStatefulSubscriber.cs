using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.EventSourcing
{
	public interface IStatefulSubscriber
	{
		Task SubscribeAsync(string streamId, long fromPosition, CancellationToken cancellationToken);
	}
}
