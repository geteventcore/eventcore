using EventCore.EventSourcing;
using Xunit;

namespace EventCore.StatefulSubscriber.Tests
{
	public class HandlingQueueItemTests
	{
		[Fact]
		public void construct()
		{
			var parallelKey = "pk";
			var subscriberEvent = new SubscriberEvent(null, 0, null, null);

			var item = new HandlingQueueItem(parallelKey, subscriberEvent);

			Assert.Equal(parallelKey, item.ParallelKey);
			Assert.Equal(subscriberEvent, item.SubscriberEvent);
		}
	}
}
