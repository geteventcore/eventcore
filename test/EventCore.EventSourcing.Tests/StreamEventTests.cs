using System;
using System.Diagnostics;
using System.Text;
using Xunit;

namespace EventCore.EventSourcing.Tests
{
	public class StreamEventTests
	{
		[Fact]
		public void construct_with_no_link()
		{
			var streamId = "sId";
			var position = (long)1;
			var eventType = "a";
			var data = Encoding.UTF8.GetBytes("{}");

			var e = new StreamEvent(streamId, position, null, eventType, data);

			Assert.Equal(streamId, e.StreamId);
			Assert.Equal(position, e.Position);
			Assert.Equal(eventType, e.EventType);
			Assert.Equal(data, e.Data); // Reference equality, not value eq.
			Assert.Null(e.Link);
			Assert.False(e.IsLink);
		}

		[Fact]
		public void construct_with_link()
		{
			var streamId = "sId";
			var position = (long)1;
			var linkStreamId = "lId";
			var linkPosition = (long)20;
			var eventType = "a";
			var data = Encoding.UTF8.GetBytes("{}");
			var link = new StreamEventLink(linkStreamId, linkPosition);

			var e = new StreamEvent(streamId, position, link, eventType, data);

			Assert.Equal(streamId, e.StreamId);
			Assert.Equal(position, e.Position);
			Assert.Equal(eventType, e.EventType);
			Assert.Equal(data, e.Data); // Reference equality, not value eq.
			Assert.Equal(link, e.Link);
			Assert.True(e.IsLink);
		}
	}
}
