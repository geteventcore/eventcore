using EventCore.AggregateRoots;
using System;
using System.Collections.Generic;
using Xunit;

namespace EventCore.AggregateRoots.Tests
{
	public class HandledCommandResultTests
	{
		[Fact]
		public void construct_with_validation_errors()
		{
			var errors = new List<string>() { "error" };
			var result = new HandledCommandResult(errors);
			Assert.False(result.IsSuccess);
			Assert.Contains(errors[0], result.ValidationErrors);
		}

		[Fact]
		public void construct_with_defaults()
		{
			var result = new HandledCommandResult();
			Assert.True(result.IsSuccess);
		}

		[Fact]
		public void create_instance_from_success()
		{
			var result = HandledCommandResult.FromSuccess();
			var resultAsync = HandledCommandResult.FromSuccessAsync().Result;
			Assert.True(result.IsSuccess);
			Assert.True(resultAsync.IsSuccess);
		}

		[Fact]
		public void create_instance_from_validation_errors()
		{
			var error = "error";
			var errors = new List<string>() { error };

			var resultFromError = HandledCommandResult.FromValidationError(error);
			var resultFromErrorAsync = HandledCommandResult.FromValidationErrorAsync(error).Result;
			var resultFromErrorIAsync = HandledCommandResult.FromValidationErrorIAsync(error).Result;
			var resultFromErrors = HandledCommandResult.FromValidationErrors(errors);
			var resultFromErrorsAsync = HandledCommandResult.FromValidationErrorsAsync(errors).Result;
			var resultFromErrorsIAsync = HandledCommandResult.FromValidationErrorsIAsync(errors).Result;

			Assert.False(resultFromError.IsSuccess);
			Assert.False(resultFromErrorAsync.IsSuccess);
			Assert.False(resultFromErrorIAsync.IsSuccess);
			Assert.False(resultFromErrors.IsSuccess);
			Assert.False(resultFromErrorsAsync.IsSuccess);
			Assert.False(resultFromErrorsIAsync.IsSuccess);

			Assert.Contains(error, resultFromError.ValidationErrors);
			Assert.Contains(error, resultFromErrorAsync.ValidationErrors);
			Assert.Contains(error, resultFromErrorIAsync.ValidationErrors);
			Assert.Contains(error, resultFromErrors.ValidationErrors);
			Assert.Contains(error, resultFromErrorsAsync.ValidationErrors);
			Assert.Contains(error, resultFromErrorsIAsync.ValidationErrors);
		}
	}
}
