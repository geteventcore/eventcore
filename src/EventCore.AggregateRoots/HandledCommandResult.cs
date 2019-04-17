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

		public static IHandledCommandResult FromSuccess() => new HandledCommandResult();
		public static Task<IHandledCommandResult> FromSuccessAsync() => Task.FromResult<IHandledCommandResult>(FromSuccess());

		public static IHandledCommandResult FromValidationError(string error) => new HandledCommandResult(new List<string>() { error });
		public static Task<IHandledCommandResult> FromValidationErrorAsync(string error) => Task.FromResult<IHandledCommandResult>(FromValidationError(error));

		public static IHandledCommandResult FromValidationErrors(IList<string> errors) => new HandledCommandResult(errors);
		public static Task<IHandledCommandResult> FromValidationErrorsAsync(IList<string> errors) => Task.FromResult<IHandledCommandResult>(FromValidationErrors(errors));
	}
}
