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

			public DerivedBusinessEvent(BusinessEventMetadata metadata, string propA) : base(metadata)
			{
				PropA = propA;
			}
		}

		[Fact]
		public void serialize_and_deserialize_a_derived_business_event()
		{
			var serializer = new JsonBusinessEventSerializer();
			var originalEvent = new DerivedBusinessEvent(new BusinessEventMetadata("abc", "123"), "A");

			var data = serializer.SerializeEvent(originalEvent);
			var deserializedEvent = (DerivedBusinessEvent)serializer.DeserializeEvent(typeof(DerivedBusinessEvent), data);
			
			Assert.NotNull(deserializedEvent);
			Assert.Equal(originalEvent.Metadata.CausalId, deserializedEvent.Metadata.CausalId);
			Assert.Equal(originalEvent.Metadata.CorrelationId, deserializedEvent.Metadata.CorrelationId);
			Assert.Equal(originalEvent.PropA, deserializedEvent.PropA);
		}
	}
}
