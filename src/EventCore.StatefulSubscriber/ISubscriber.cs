using EventCore.EventSourcing;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public interface ISubscriber
	{
		Task<IDictionary<string, long?>> GetEndsOfSubscriptionAsync();
		Task SubscribeAsync(CancellationToken cancellationToken);
		Task ResetStreamStatesAsync();
		Task ClearStreamStateErrorsAsync();
	}
}
