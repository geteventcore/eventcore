using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.IO;

namespace EventCore.EventSourcing
{
	public class BsonBusinessEventSerializer : IBusinessEventSerializer
	{
		public BusinessEvent DeserializeEvent(Type t, byte[] data)
		{
			BusinessEvent e = null;

			using (var ms = new MemoryStream(data))
			{
				using (var reader = new BsonDataReader(ms))
				{
					var serializer = new JsonSerializer();
					e = (BusinessEvent)serializer.Deserialize(reader, t);
				}
			}
			return e;
		}

		public byte[] SerializeEvent<T>(T e) where T : BusinessEvent
		{
			byte[] data;

			using (var ms = new MemoryStream())
			{
				using (var writer = new BsonDataWriter(ms))
				{
					var serializer = new JsonSerializer();
					serializer.Serialize(writer, e);
				}
				data = ms.ToArray();
			}
			return data;
		}
	}
}
