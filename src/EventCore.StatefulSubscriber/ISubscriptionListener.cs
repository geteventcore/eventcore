﻿using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public interface ISubscriptionListener
	{
		Task ListenAsync(string regionId, string subscriptionStreamId, CancellationToken cancellationToken);
	}
}
