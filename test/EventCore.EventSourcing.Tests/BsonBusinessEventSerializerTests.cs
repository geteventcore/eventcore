using System;
using System.Diagnostics;
using System.Text;
using Xunit;

namespace EventCore.EventSourcing.Tests
{
	public class BsonBusinessEventSerializerTests
	{
		private class DerivedBusinessEvent : BusinessEvent
		{
			public string PropA { get; }

			public DerivedBusinessEvent(string causalId, string correlationId, string propA) : base(causalId, correlationId)
			{
				PropA = propA;
			}
		}

		[Fact]
		public void serialize_and_deserialize_a_derived_business_event()
		{
			var serializer = new BsonBusinessEventSerializer();
			var originalEvent = new DerivedBusinessEvent("abc", "123", "A");

			var data = serializer.SerializeEvent(originalEvent);
			var deserializedEvent = (DerivedBusinessEvent)serializer.DeserializeEvent(typeof(DerivedBusinessEvent), data);
			
			Assert.NotNull(deserializedEvent);
			Assert.Equal(originalEvent.CausalId, deserializedEvent.CausalId);
			Assert.Equal(originalEvent.CorrelationId, deserializedEvent.CorrelationId);
			Assert.Equal(originalEvent.PropA, deserializedEvent.PropA);
		}
	}
}
