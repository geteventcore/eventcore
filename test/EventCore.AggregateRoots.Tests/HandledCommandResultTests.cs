using EventCore.AggregateRoots;
using System;
using System.Collections.Generic;
using Xunit;

namespace EventCore.AggregateRoots.Tests
{
	public class HandledCommandResultTests
	{
		[Fact]
		public void should_construct_with_validation_errors_and_serialized_state()
		{
			var errors = new List<string>() { "error" };
			var state = "{}";
			var result = new HandledCommandResult(errors, state);
			Assert.False(result.IsSuccess);
			Assert.Contains(errors[0], result.ValidationErrors);
			Assert.Equal(state, result.SerializedState);
		}

		[Fact]
		public void should_construct_with_serialized_state()
		{
			var state = "{}";
			var result = new HandledCommandResult(state);
			Assert.True(result.IsSuccess);
			Assert.Equal(state, result.SerializedState);
		}

		[Fact]
		public void should_create_instance_from_success()
		{
			var result = HandledCommandResult.FromSuccess();
			var resultAsync = HandledCommandResult.FromSuccessAsync().Result;
			Assert.True(result.IsSuccess);
			Assert.True(resultAsync.IsSuccess);
		}

		[Fact]
		public void should_create_instance_from_validation_errors()
		{
			var error = "error";
			var errors = new List<string>() { error };

			var resultFromError = HandledCommandResult.FromValidationError(error);
			var resultFromErrorAsync = HandledCommandResult.FromValidationErrorAsync(error).Result;
			var resultFromErrors = HandledCommandResult.FromValidationErrors(errors);
			var resultFromErrorsAsync = HandledCommandResult.FromValidationErrorsAsync(errors).Result;

			Assert.False(resultFromError.IsSuccess);
			Assert.False(resultFromErrorAsync.IsSuccess);
			Assert.False(resultFromErrors.IsSuccess);
			Assert.False(resultFromErrorsAsync.IsSuccess);

			Assert.Contains(error, resultFromError.ValidationErrors);
			Assert.Contains(error, resultFromErrorAsync.ValidationErrors);
			Assert.Contains(error, resultFromErrors.ValidationErrors);
			Assert.Contains(error, resultFromErrorsAsync.ValidationErrors);
		}
	}
}
