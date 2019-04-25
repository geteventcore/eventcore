using System;
using System.Collections.Generic;
using EventCore.EventSourcing;
using Moq;
using Xunit;

namespace EventCore.AggregateRoots.Tests
{
	public class CommandEventsResultTests
	{
		[Fact]
		public void construct_with_defaults()
		{
			var result = new CommandEventsResult();
			Assert.Empty(result.Events); // More accurate than NotNull.
		}

		[Fact]
		public void construct_with_events()
		{
			var mockEvent = new Mock<IBusinessEvent>();
			var events = new List<IBusinessEvent>() { mockEvent.Object };
			var result = new CommandEventsResult(events);
			Assert.Contains(mockEvent.Object, result.Events);
		}

		[Fact]
		public void create_instance_from_events()
		{
			var mockEvent = new Mock<IBusinessEvent>();
			var events = new List<IBusinessEvent>() { mockEvent.Object };

			var resultFromEvent = CommandEventsResult.FromEvent(mockEvent.Object);
			var resultFromEventAsync = CommandEventsResult.FromEventAsync(mockEvent.Object).Result;
			var resultFromEventIAsync = CommandEventsResult.FromEventIAsync(mockEvent.Object).Result;
			var resultFromEvents = CommandEventsResult.FromEvents(events);
			var resultFromEventsAsync = CommandEventsResult.FromEventsAsync(events).Result;
			var resultFromEventsIAsync = CommandEventsResult.FromEventsIAsync(events).Result;

			Assert.Contains(mockEvent.Object, resultFromEvent.Events);
			Assert.Contains(mockEvent.Object, resultFromEventAsync.Events);
			Assert.Contains(mockEvent.Object, resultFromEventIAsync.Events);
			Assert.Contains(mockEvent.Object, resultFromEvents.Events);
			Assert.Contains(mockEvent.Object, resultFromEventsAsync.Events);
			Assert.Contains(mockEvent.Object, resultFromEventsIAsync.Events);
		}
	}
}
