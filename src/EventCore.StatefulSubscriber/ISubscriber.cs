using EventCore.EventSourcing;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public interface ISubscriber
	{
		Task SubscribeAsync(CancellationToken cancellationToken);
		Task ResetStreamStatesAsync();
		Task ClearStreamStateErrorsAsync();
	}
}
