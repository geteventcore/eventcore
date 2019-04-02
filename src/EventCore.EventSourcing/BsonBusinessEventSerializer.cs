using System;

namespace EventCore.EventSourcing
{
	public class BsonBusinessEventSerializer : IBusinessEventSerializer
	{
		public BusinessEvent DerializeEvent(Type t, byte[] payload)
		{
			throw new NotImplementedException();
		}

		public byte[] SerializeEvent<T>(T e) where T : BusinessEvent
		{
			throw new NotImplementedException();
		}
	}
}
