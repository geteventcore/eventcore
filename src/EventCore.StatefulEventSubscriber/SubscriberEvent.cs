using EventCore.EventSourcing;
using System;

namespace EventCore.StatefulEventSubscriber
{
	public class SubscriberEvent
	{
		public readonly string StreamId;
		public readonly long Position;
		public readonly Type EventType;
		public readonly bool IsResolved;
		public readonly BusinessEvent ResolvedEvent;

		public SubscriberEvent(string streamId, long position, BusinessEvent resolvedEvent)
		{
			StreamId = streamId;
			Position = position;
			EventType = resolvedEvent?.GetType();
			IsResolved = resolvedEvent != null;
			ResolvedEvent = resolvedEvent;
		}
	}
}
