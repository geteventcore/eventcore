using System.Text;
using Xunit;

namespace EventCore.StatefulEventSubscriber.Tests
{
	public class SubscriberOptionsTests
	{
		[Fact]
		public void construct()
		{
			var maxParallelHandlerExecutions = (int)5;
			var streamId = "sId";
			var regionIds = new string[] { "r1", "r2" };

			var options = new SubscriberOptions(maxParallelHandlerExecutions, streamId, regionIds);

			Assert.Equal(maxParallelHandlerExecutions, options.MaxParallelHandlerExecutions);
			Assert.Equal(streamId, options.StreamId);
			Assert.Equal(regionIds, options.RegionIds); // Reference equality, fyi.
		}
	}
}
