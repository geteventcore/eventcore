using System;

namespace EventCore.EventSourcing
{
	public class StreamEvent
	{
		public string StreamId { get; }
		public long Position { get; }
		public bool IsLink { get => Link != null; }
		public StreamEventLink Link { get; }
		public string EventType { get; }
		public byte[] Data { get; }

		public StreamEvent(string streamId, long position, StreamEventLink link, string eventType, byte[] data)
		{
			StreamId = streamId;
			Position = position;
			Link = link;
			EventType = eventType;
			Data = data;
		}
	}
}
