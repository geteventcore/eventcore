using System;
using System.Collections.Generic;
using System.Linq;

namespace EventCore.StatefulEventSubscriber
{
	public class SubscriberOptions
	{
		public readonly IList<SubscriptionStreamId> SubscriptionStreamIds;

		public SubscriberOptions(IList<SubscriptionStreamId> subscriptionStreamIds)
		{
			SubscriptionStreamIds = subscriptionStreamIds;
		}
	}
}
