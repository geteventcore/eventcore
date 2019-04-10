using EventCore.EventSourcing;
using Xunit;

namespace EventCore.StatefulEventSubscriber.Tests
{
	public class HandlingManagerTaskCollectionTests
	{
		[Fact]
		public void construct()
		{
			var parallelKey = "pk";
			var subscriberEvent = new SubscriberEvent("sId", 1, new BusinessEvent(BusinessMetadata.Empty));
			
			var item = new HandlingQueueItem(parallelKey, subscriberEvent);

			Assert.Equal(parallelKey, item.ParallelKey);
			Assert.Equal(subscriberEvent, item.SubscriberEvent);
		}
	}
}
