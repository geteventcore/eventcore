using System;

namespace EventCore.EventSourcing
{
	public class StreamEventLink
	{
		public readonly string StreamId;
		public readonly long Position;

		public StreamEventLink(string streamId, long position)
		{
			StreamId = streamId;
			Position = position;
		}
	}
}
