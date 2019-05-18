using System.Text;
using EventCore.EventSourcing;
using Xunit;

namespace EventCore.StatefulSubscriber.Tests
{
	public class SubscriberEventTests
	{
		private class TestBusinessEvent : BusinessEvent
		{
			public TestBusinessEvent() : base(BusinessEventMetadata.Empty) { }
		}

		[Fact]
		public void construct_with_subscription_stream_info()
		{
			var streamId = "str";
			var position = 1;
			var eventType = "x";
			var subscriptionStreamId = "sub";
			var subscriptionPosition = 20;

			var e = new SubscriberEvent(streamId, position, subscriptionStreamId, subscriptionPosition, eventType, null);

			Assert.Equal(streamId, e.StreamId);
			Assert.Equal(position, e.Position);
			Assert.Equal(subscriptionStreamId, e.SubscriptionStreamId);
			Assert.Equal(subscriptionPosition, e.SubscriptionPosition);
			Assert.Equal(eventType, e.EventType);
			Assert.Null(e.ResolvedEventType);
			Assert.Null(e.ResolvedEvent);
			Assert.False(e.IsResolved);
		}

		[Fact]
		public void construct_with_event_not_resolved()
		{
			var streamId = "s";
			var position = 1;
			var eventType = "x";

			var e = new SubscriberEvent(streamId, position, eventType, null);

			Assert.Equal(streamId, e.StreamId);
			Assert.Equal(position, e.Position);
			Assert.Equal(streamId, e.SubscriptionStreamId);
			Assert.Equal(position, e.SubscriptionPosition);
			Assert.Equal(eventType, e.EventType);
			Assert.Null(e.ResolvedEventType);
			Assert.Null(e.ResolvedEvent);
			Assert.False(e.IsResolved);
		}

		[Fact]
		public void construct_with_event_resolved()
		{
			var streamId = "s";
			var position = 1;
			var eventType = "x";
			var resolvedEventType = typeof(TestBusinessEvent);
			var resolvedEvent = new TestBusinessEvent();

			var e = new SubscriberEvent(streamId, position, eventType, resolvedEvent);

			Assert.Equal(streamId, e.StreamId);
			Assert.Equal(position, e.Position);
			Assert.Equal(streamId, e.SubscriptionStreamId);
			Assert.Equal(position, e.SubscriptionPosition);
			Assert.Equal(eventType, e.EventType);
			Assert.Equal(resolvedEventType, e.ResolvedEventType);
			Assert.Equal(resolvedEvent, e.ResolvedEvent);
			Assert.True(e.IsResolved);
		}
	}
}
