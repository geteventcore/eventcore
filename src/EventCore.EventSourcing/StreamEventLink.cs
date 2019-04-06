using System;

namespace EventCore.EventSourcing
{
	public class StreamEventLink
	{
		public string StreamId { get; }
		public long Position { get; }

		public StreamEventLink(string streamId, long position)
		{
			StreamId = streamId;
			Position = position;
		}
	}
}
