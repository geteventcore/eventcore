using Xunit;

namespace EventCore.EventSourcing.Tests
{
	public class StreamEventLinkTests
	{
		[Fact]
		public void construct()
		{
			var streamId = "sId";
			var position = (long)1;

			var e = new StreamEventLink(streamId, position);

			Assert.Equal(streamId, e.StreamId);
			Assert.Equal(position, e.Position);
		}
	}
}
