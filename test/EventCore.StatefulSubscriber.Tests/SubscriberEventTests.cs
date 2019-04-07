using System.Text;
using EventCore.EventSourcing;
using Xunit;

namespace EventCore.StatefulSubscriber.Tests
{
	public class SubscriberEventTests
	{
		private class TestBusinessEvent : BusinessEvent
		{
			public TestBusinessEvent(BusinessMetadata metadata) : base(metadata)
			{
			}
		}

		[Fact]
		public void construct_with_event_not_resolved()
		{
			var streamId = "sId";
			var position = (long)1;

			var e = new SubscriberEvent(streamId, position, null);

			Assert.Equal(streamId, e.StreamId);
			Assert.Equal(position, e.Position);
			Assert.Null(e.EventType);
			Assert.Null(e.ResolvedEvent);
			Assert.False(e.IsResolved);
		}

		[Fact]
		public void construct_with_event_resolved()
		{
			var streamId = "sId";
			var position = (long)1;
			var eventType = typeof(TestBusinessEvent);
			var resolvedEvent = new TestBusinessEvent(BusinessMetadata.Empty);

			var e = new SubscriberEvent(streamId, position, resolvedEvent);

			Assert.Equal(streamId, e.StreamId);
			Assert.Equal(position, e.Position);
			Assert.Equal(eventType, e.EventType);
			Assert.Equal(resolvedEvent, e.ResolvedEvent);
			Assert.True(e.IsResolved);
		}
	}
}
