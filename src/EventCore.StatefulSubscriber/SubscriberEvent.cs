using EventCore.EventSourcing;
using System;

namespace EventCore.StatefulSubscriber
{
	public class SubscriberEvent
	{
		public readonly string StreamId;
		public readonly long Position;
		public readonly string SubscriptionStreamId;
		public readonly long SubscriptionPosition;
		public readonly bool IsResolved;
		public readonly string EventType;
		public readonly Type ResolvedEventType;
		public readonly IBusinessEvent ResolvedEvent;

		public SubscriberEvent(string streamId, long position, string eventType, IBusinessEvent resolvedEvent)
		{
			StreamId = streamId;
			Position = position;
			SubscriptionStreamId = streamId;
			SubscriptionPosition = position;
			EventType = eventType;
			IsResolved = resolvedEvent != null;
			ResolvedEventType = resolvedEvent?.GetType();
			ResolvedEvent = resolvedEvent;
		}

		public SubscriberEvent(string streamId, long position, string subscriptionStreamId, long subscriptionPosition, string eventType, IBusinessEvent resolvedEvent)
		{
			StreamId = streamId;
			Position = position;
			SubscriptionStreamId = subscriptionStreamId;
			SubscriptionPosition = subscriptionPosition;
			EventType = eventType;
			IsResolved = resolvedEvent != null;
			ResolvedEventType = resolvedEvent?.GetType();
			ResolvedEvent = resolvedEvent;
		}
	}
}
