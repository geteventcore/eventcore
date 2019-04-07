using System;

namespace EventCore.EventSourcing
{
	public class StreamEvent
	{
		public readonly string StreamId;
		public readonly long Position;
		public readonly bool IsLink;
		public readonly StreamEventLink Link;
		public readonly string EventType;
		public readonly byte[] Data;

		public StreamEvent(string streamId, long position, StreamEventLink link, string eventType, byte[] data)
		{
			StreamId = streamId;
			Position = position;
			IsLink = link != null;
			Link = link;
			EventType = eventType;
			Data = data;
		}
	}
}
