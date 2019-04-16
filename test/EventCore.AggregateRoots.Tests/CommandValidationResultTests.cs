using System;
using System.Collections.Generic;
using Xunit;

namespace EventCore.AggregateRoots.Tests
{
	public class CommandValidationResultTests
	{
		[Fact]
		public void construct_with_defaults()
		{
			var result = new CommandValidationResult();
			Assert.True(result.IsValid);
			Assert.Empty(result.Errors);
		}

		[Fact]
		public void construct_with_errors()
		{
			var errors = new List<string>() { "error" };
			var result = new CommandValidationResult(errors);
			Assert.False(result.IsValid);
			Assert.Contains(errors[0], result.Errors);
		}

		[Fact]
		public void create_instance_from_valid()
		{
			var result = CommandValidationResult.FromValid();
			var resultAsync = CommandValidationResult.FromValidAsync().Result;
			Assert.True(result.IsValid);
			Assert.True(resultAsync.IsValid);
		}

		[Fact]
		public void create_instance_from_errors()
		{
			var error = "error";
			var errors = new List<string>() { error };

			var resultFromError = CommandValidationResult.FromError(error);
			var resultFromErrorAsync = CommandValidationResult.FromErrorAsync(error).Result;
			var resultFromErrors = CommandValidationResult.FromErrors(errors);
			var resultFromErrorsAsync = CommandValidationResult.FromErrorsAsync(errors).Result;

			Assert.False(resultFromError.IsValid);
			Assert.False(resultFromErrorAsync.IsValid);
			Assert.False(resultFromErrors.IsValid);
			Assert.False(resultFromErrorsAsync.IsValid);

			Assert.Contains(error, resultFromError.Errors);
			Assert.Contains(error, resultFromErrorAsync.Errors);
			Assert.Contains(error, resultFromErrors.Errors);
			Assert.Contains(error, resultFromErrorsAsync.Errors);
		}
	}
}
