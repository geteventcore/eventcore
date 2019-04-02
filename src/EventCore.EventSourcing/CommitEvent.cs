using System;

namespace EventCore.EventSourcing
{
	public class CommitEvent
	{
		public string EventType { get; }
		public byte[] Payload { get; }

		public CommitEvent(string eventType, byte[] payload)
		{
			EventType = eventType;
			Payload = payload;
		}
	}
}
