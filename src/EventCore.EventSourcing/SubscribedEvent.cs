using System;

namespace EventCore.EventSourcing
{
	public class SubscribedEvent
	{
		public string BaseStreamId { get; }
		public long BasePosition { get; }
		public string SubscribedStreamId { get; }
		public long SubscribedPosition { get; }
		public string EventType { get; }
		public byte[] Payload { get; }

		public SubscribedEvent(string baseStreamId, long basePosition, string subscribedStreamId, long subscribedPosition, string eventType, byte[] payload)
		{
			BaseStreamId = baseStreamId;
			BasePosition = basePosition;
			SubscribedStreamId = subscribedStreamId;
			SubscribedPosition = subscribedPosition;
			EventType = eventType;
			Payload = payload;
		}
	}
}
