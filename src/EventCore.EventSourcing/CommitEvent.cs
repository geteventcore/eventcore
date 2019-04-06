using System;

namespace EventCore.EventSourcing
{
	public class CommitEvent
	{
		public string EventType { get; }
		public byte[] Data { get; }

		public CommitEvent(string eventType, byte[] data)
		{
			EventType = eventType;
			Data = data;
		}
	}
}
