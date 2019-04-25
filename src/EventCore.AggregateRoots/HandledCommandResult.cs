using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public class HandledCommandResult : IHandledCommandResult
	{
		public bool IsSuccess { get; }
		public IImmutableList<string> ValidationErrors { get; }

		public HandledCommandResult(IList<string> errors)
		{
			IsSuccess = false;
			ValidationErrors = errors.ToImmutableList();
		}

		public HandledCommandResult()
		{
			IsSuccess = true;
		}

		public static HandledCommandResult FromSuccess() => new HandledCommandResult();
		public static Task<HandledCommandResult> FromSuccessAsync() => Task.FromResult<HandledCommandResult>(FromSuccess());
		public static Task<IHandledCommandResult> FromSuccessIsync() => Task.FromResult<IHandledCommandResult>(FromSuccess());

		public static HandledCommandResult FromValidationError(string error) => new HandledCommandResult(new List<string>() { error });
		public static Task<HandledCommandResult> FromValidationErrorAsync(string error) => Task.FromResult<HandledCommandResult>(FromValidationError(error));
		public static Task<IHandledCommandResult> FromValidationErrorIAsync(string error) => Task.FromResult<IHandledCommandResult>(FromValidationError(error));

		public static HandledCommandResult FromValidationErrors(IList<string> errors) => new HandledCommandResult(errors);
		public static Task<HandledCommandResult> FromValidationErrorsAsync(IList<string> errors) => Task.FromResult<HandledCommandResult>(FromValidationErrors(errors));
		public static Task<IHandledCommandResult> FromValidationErrorsIAsync(IList<string> errors) => Task.FromResult<IHandledCommandResult>(FromValidationErrors(errors));
	}
}
