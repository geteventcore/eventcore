using System;

namespace EventCore.EventSourcing
{
	public class CommitEvent
	{
		public readonly string EventType;
		public readonly byte[] Data;

		public CommitEvent(string eventType, byte[] data)
		{
			EventType = eventType;
			Data = data;
		}
	}
}
