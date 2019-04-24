using Xunit;

namespace EventCore.EventSourcing.StatefulSubscriber.Tests
{
	public class SubscriptionStreamIdTests
	{
		[Fact]
		public void construct()
		{
			var regionId = "r";
			var streamId = "s";

			var subStreamId = new SubscriptionStreamId(regionId, streamId);

			Assert.Equal(regionId, subStreamId.RegionId);
			Assert.Equal(streamId, subStreamId.StreamId);
		}
	}
}
