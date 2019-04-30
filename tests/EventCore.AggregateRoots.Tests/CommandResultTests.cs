using EventCore.AggregateRoots;
using EventCore.EventSourcing;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace EventCore.AggregateRoots.Tests
{
	public class CommandResultTests
	{
		[Fact]
		public void construct_with_errors()
		{
			var errors = new List<string>() { "error" };
			var result = new CommandResult(errors);
			Assert.False(result.IsSuccess);
			Assert.Contains(errors[0], result.Errors);
		}

		[Fact]
		public void construct_with_events()
		{
			var mockEvent = new Mock<IBusinessEvent>();
			var events = new List<IBusinessEvent>() { mockEvent.Object };
			var result = new CommandResult(events);
			Assert.Contains(mockEvent.Object, result.Events);
		}

		[Fact]
		public void create_instance_from_validation_errors()
		{
			var error = "error";
			var errors = new List<string>() { error };

			var resultFromError = CommandResult.FromError(error);
			var resultFromErrorAsync = CommandResult.FromErrorAsync(error).Result;
			var resultFromErrorIAsync = CommandResult.FromErrorIAsync(error).Result;
			var resultFromErrors = CommandResult.FromErrors(errors);
			var resultFromErrorsAsync = CommandResult.FromErrorsAsync(errors).Result;
			var resultFromErrorsIAsync = CommandResult.FromErrorsIAsync(errors).Result;

			Assert.False(resultFromError.IsSuccess);
			Assert.False(resultFromErrorAsync.IsSuccess);
			Assert.False(resultFromErrorIAsync.IsSuccess);
			Assert.False(resultFromErrors.IsSuccess);
			Assert.False(resultFromErrorsAsync.IsSuccess);
			Assert.False(resultFromErrorsIAsync.IsSuccess);

			Assert.Contains(error, resultFromError.Errors);
			Assert.Contains(error, resultFromErrorAsync.Errors);
			Assert.Contains(error, resultFromErrorIAsync.Errors);
			Assert.Contains(error, resultFromErrors.Errors);
			Assert.Contains(error, resultFromErrorsAsync.Errors);
			Assert.Contains(error, resultFromErrorsIAsync.Errors);
		}

		[Fact]
		public void create_instance_from_events()
		{
			var mockEvent = new Mock<IBusinessEvent>();
			var events = new List<IBusinessEvent>() { mockEvent.Object };

			var resultFromEvent = CommandResult.FromEvent(mockEvent.Object);
			var resultFromEventAsync = CommandResult.FromEventAsync(mockEvent.Object).Result;
			var resultFromEventIAsync = CommandResult.FromEventIAsync(mockEvent.Object).Result;
			var resultFromEvents = CommandResult.FromEvents(events);
			var resultFromEventsAsync = CommandResult.FromEventsAsync(events).Result;
			var resultFromEventsIAsync = CommandResult.FromEventsIAsync(events).Result;

			Assert.True(resultFromEvent.IsSuccess);
			Assert.True(resultFromEventAsync.IsSuccess);
			Assert.True(resultFromEventIAsync.IsSuccess);
			Assert.True(resultFromEvents.IsSuccess);
			Assert.True(resultFromEventsAsync.IsSuccess);
			Assert.True(resultFromEventsIAsync.IsSuccess);

			Assert.Contains(mockEvent.Object, resultFromEvent.Events);
			Assert.Contains(mockEvent.Object, resultFromEventAsync.Events);
			Assert.Contains(mockEvent.Object, resultFromEventIAsync.Events);
			Assert.Contains(mockEvent.Object, resultFromEvents.Events);
			Assert.Contains(mockEvent.Object, resultFromEventsAsync.Events);
			Assert.Contains(mockEvent.Object, resultFromEventsIAsync.Events);
		}
	}
}
