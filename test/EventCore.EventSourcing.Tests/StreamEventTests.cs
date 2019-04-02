using System;
using System.Diagnostics;
using System.Text;
using Xunit;

namespace EventCore.EventSourcing.Tests
{
	public class StreamEventTests
	{
		[Fact]
		public void construct()
		{
			var streamId = "sId";
			var position = (long)1;
			var eventType = "a";
			var payload = Encoding.UTF8.GetBytes("{}");

			var e = new StreamEvent(streamId, position, eventType, payload);

			Assert.Equal(e.StreamId, streamId);
			Assert.Equal(e.Position, position);
			Assert.Equal(e.EventType, eventType);
			Assert.Equal(e.Payload, payload); // Reference equality, not value eq.
		}
	}
}
