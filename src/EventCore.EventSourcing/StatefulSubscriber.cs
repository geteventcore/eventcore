using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.EventSourcing
{
	public class StatefulSubscriber : IStatefulSubscriber
	{
		public Task SubscribeAsync(string streamId, long fromPosition, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
