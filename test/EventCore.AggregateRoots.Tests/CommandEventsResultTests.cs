using System;
using System.Collections.Generic;
using EventCore.EventSourcing;
using Xunit;

namespace EventCore.AggregateRoots.Tests
{
	public class CommandEventsResultTests
	{
		private class TestBusinessEvent : BusinessEvent
		{
			public TestBusinessEvent(BusinessEventMetadata metadata) : base(metadata) { }
		}

		[Fact]
		public void construct_with_defaults()
		{
			var result = new CommandEventsResult();
			Assert.Empty(result.Events); // More accurate than NotNull.
		}

		[Fact]
		public void construct_with_events()
		{
			var e = new TestBusinessEvent(BusinessEventMetadata.Empty);
			var events = new List<BusinessEvent>() { e };
			var result = new CommandEventsResult(events);
			Assert.Contains(e, result.Events);
		}

		[Fact]
		public void create_instance_from_events()
		{
			var e = new TestBusinessEvent(BusinessEventMetadata.Empty);
			var events = new List<BusinessEvent>() { e };

			var resultFromEvent = CommandEventsResult.FromEvent(e);
			var resultFromEventAsync = CommandEventsResult.FromEventAsync(e).Result;
			var resultFromEvents = CommandEventsResult.FromEvents(events);
			var resultFromEventsAsync = CommandEventsResult.FromEventsAsync(events).Result;

			Assert.Contains(e, resultFromEvent.Events);
			Assert.Contains(e, resultFromEventAsync.Events);
			Assert.Contains(e, resultFromEvents.Events);
			Assert.Contains(e, resultFromEventsAsync.Events);
		}
	}
}
