using System;
using System.Diagnostics;
using System.Text;
using Xunit;

namespace EventCore.EventSourcing.Tests
{
	public class SubscribedEventTests
	{
		[Fact]
		public void construct()
		{
			var baseStreamId = "sId";
			var basePosition = (long)1;
			var subscribedStreamId = "x1";
			var subscribedPosition = (long)1;
			var eventType = "a";
			var payload = Encoding.UTF8.GetBytes("{}");

			var e = new SubscribedEvent(baseStreamId, basePosition, subscribedStreamId, subscribedPosition, eventType, payload);

			Assert.Equal(e.BaseStreamId, baseStreamId);
			Assert.Equal(e.BasePosition, basePosition);
			Assert.Equal(e.SubscribedStreamId, subscribedStreamId);
			Assert.Equal(e.SubscribedPosition, subscribedPosition);
			Assert.Equal(e.EventType, eventType);
			Assert.Equal(e.Payload, payload); // Reference equality, not value eq.
		}
	}
}
