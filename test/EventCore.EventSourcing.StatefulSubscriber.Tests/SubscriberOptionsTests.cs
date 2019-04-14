using System.Collections.Generic;
using Xunit;

namespace EventCore.EventSourcing.StatefulSubscriber.Tests
{
	public class SubscriberOptionsTests
	{
		[Fact]
		public void construct()
		{
			var subscriptionStreamIds = new List<SubscriptionStreamId>();

			var options = new SubscriberOptions(subscriptionStreamIds);

			Assert.Equal(subscriptionStreamIds, options.SubscriptionStreamIds);
		}
	}
}