using System;

namespace EventCore.EventSourcing
{
	public class UnresolvedBusinessEvent
	{
		public readonly string EventType;
		public readonly byte[] Data;

		public UnresolvedBusinessEvent(string eventType, byte[] data)
		{
			EventType = eventType;
			Data = data;
		}
	}
}
