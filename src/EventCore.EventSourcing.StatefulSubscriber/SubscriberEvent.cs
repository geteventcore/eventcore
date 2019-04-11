using EventCore.EventSourcing;
using System;

namespace EventCore.EventSourcing.StatefulSubscriber
{
	public class SubscriberEvent
	{
		public readonly string StreamId;
		public readonly long Position;
		public readonly string SubscriptionStreamId;
		public readonly long SubscriptionPosition;
		public readonly Type EventType;
		public readonly bool IsResolved;
		public readonly BusinessEvent ResolvedEvent;

		public SubscriberEvent(string streamId, long position, BusinessEvent resolvedEvent)
		{
			StreamId = streamId;
			Position = position;
			SubscriptionStreamId = streamId;
			SubscriptionPosition = position;
			EventType = resolvedEvent?.GetType();
			IsResolved = resolvedEvent != null;
			ResolvedEvent = resolvedEvent;
		}

		public SubscriberEvent(string streamId, long position, string subscriptionStreamId, long subscriptionPosition, BusinessEvent resolvedEvent)
		{
			StreamId = streamId;
			Position = position;
			SubscriptionStreamId = subscriptionStreamId;
			SubscriptionPosition = subscriptionPosition;
			EventType = resolvedEvent?.GetType();
			IsResolved = resolvedEvent != null;
			ResolvedEvent = resolvedEvent;
		}
	}
}
