using EventCore.EventSourcing;
using Xunit;

namespace EventCore.StatefulEventSubscriber.Tests
{
	public class HandlingQueueItemTests
	{
		private class TestBusinessEvent : BusinessEvent
		{
			public TestBusinessEvent(BusinessEventMetadata metadata) : base(metadata) { }
		}

		[Fact]
		public void construct()
		{
			var parallelKey = "pk";
			var subscriberEvent = new SubscriberEvent("sId", 1, new TestBusinessEvent(BusinessEventMetadata.Empty));
			
			var item = new HandlingQueueItem(parallelKey, subscriberEvent);

			Assert.Equal(parallelKey, item.ParallelKey);
			Assert.Equal(subscriberEvent, item.SubscriberEvent);
		}
	}
}
