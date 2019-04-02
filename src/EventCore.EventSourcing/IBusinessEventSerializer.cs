using System;

namespace EventCore.EventSourcing
{
	public interface IBusinessEventSerializer
	{
		byte[] SerializeEvent<T>(T e) where T : BusinessEvent;
		BusinessEvent DeserializeEvent(Type t, byte[] data);
	}
}
