using System;

namespace EventCore.EventSourcing
{
	public class StreamEvent
	{
		public string StreamId { get; }
		public long Position { get; }
		public string EventType { get; }
		public byte[] Payload { get; }

		public StreamEvent(string streamId, long position, string eventType, byte[] payload)
		{
			StreamId = streamId;
			Position = position;
			EventType = eventType;
			Payload = payload;
		}
	}
}
