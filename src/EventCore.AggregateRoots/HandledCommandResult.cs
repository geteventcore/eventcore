using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public class HandledCommandResult : IHandledCommandResult
	{
		public bool IsSuccess { get; }
		public IImmutableList<string> ValidationErrors { get; }
		public string SerializedState { get; } // Agg root may want to preserve serialized state event if command has validation errors.

		public HandledCommandResult(IList<string> errors, string serializedState = null)
		{
			IsSuccess = false;
			ValidationErrors = errors.ToImmutableList();
			SerializedState = serializedState;
		}

		public HandledCommandResult(string serializedState = null)
		{
			IsSuccess = true;
			SerializedState = serializedState;
		}

		public static IHandledCommandResult FromSuccess() => new HandledCommandResult();
		public static Task<IHandledCommandResult> FromSuccessAsync() => Task.FromResult<IHandledCommandResult>(FromSuccess());

		public static IHandledCommandResult FromValidationError(string error, string serializedState = null) => new HandledCommandResult(new List<string>() { error }, serializedState);
		public static Task<IHandledCommandResult> FromValidationErrorAsync(string error, string serializedState = null) => Task.FromResult<IHandledCommandResult>(FromValidationError(error, serializedState));

		public static IHandledCommandResult FromValidationErrors(IList<string> errors, string serializedState = null) => new HandledCommandResult(errors, serializedState);
		public static Task<IHandledCommandResult> FromValidationErrorsAsync(IList<string> errors, string serializedState = null) => Task.FromResult<IHandledCommandResult>(FromValidationErrors(errors, serializedState));
	}
}
