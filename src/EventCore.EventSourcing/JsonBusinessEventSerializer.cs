using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.IO;
using System.Text;

namespace EventCore.EventSourcing
{
	public class JsonBusinessEventSerializer : IBusinessEventSerializer
	{
		public BusinessEvent DeserializeEvent(Type t, byte[] data)
		{
			var json = Encoding.Unicode.GetString(data);
			if (string.IsNullOrWhiteSpace(json))
			{
				return null;
			}
			return (BusinessEvent)JsonConvert.DeserializeObject(json, t);
		}

		public byte[] SerializeEvent<T>(T e) where T : BusinessEvent
		{
			return Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(e));
		}
	}
}
