using System.Text;
using EventCore.EventSourcing;
using Xunit;

namespace EventCore.StatefulEventSubscriber.Tests
{
	public class SubscriberEventTests
	{
		private class TestBusinessEvent : BusinessEvent
		{
			public TestBusinessEvent(BusinessEventMetadata metadata) : base(metadata)
			{
			}
		}

		[Fact]
		public void construct_with_subscription_stream_info()
		{
			var streamId = "str";
			var position = 1;
			var subscriptionStreamId = "sub";
			var subscriptionPosition = 20;

			var e = new SubscriberEvent(streamId, position, subscriptionStreamId, subscriptionPosition, null);

			Assert.Equal(streamId, e.StreamId);
			Assert.Equal(position, e.Position);
			Assert.Equal(subscriptionStreamId, e.SubscriptionStreamId);
			Assert.Equal(subscriptionPosition, e.SubscriptionPosition);
			Assert.Null(e.EventType);
			Assert.Null(e.ResolvedEvent);
			Assert.False(e.IsResolved);
		}

		[Fact]
		public void construct_with_event_not_resolved()
		{
			var streamId = "s";
			var position = 1;

			var e = new SubscriberEvent(streamId, position, null);

			Assert.Equal(streamId, e.StreamId);
			Assert.Equal(position, e.Position);
			Assert.Equal(streamId, e.SubscriptionStreamId);
			Assert.Equal(position, e.SubscriptionPosition);
			Assert.Null(e.EventType);
			Assert.Null(e.ResolvedEvent);
			Assert.False(e.IsResolved);
		}

		[Fact]
		public void construct_with_event_resolved()
		{
			var streamId = "s";
			var position = 1;
			var eventType = typeof(TestBusinessEvent);
			var resolvedEvent = new TestBusinessEvent(BusinessEventMetadata.Empty);

			var e = new SubscriberEvent(streamId, position, resolvedEvent);

			Assert.Equal(streamId, e.StreamId);
			Assert.Equal(position, e.Position);
			Assert.Equal(streamId, e.SubscriptionStreamId);
			Assert.Equal(position, e.SubscriptionPosition);
			Assert.Equal(eventType, e.EventType);
			Assert.Equal(resolvedEvent, e.ResolvedEvent);
			Assert.True(e.IsResolved);
		}
	}
}
