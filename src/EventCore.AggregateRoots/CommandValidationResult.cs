using EventCore.EventSourcing;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public class CommandValidationResult : ICommandValidationResult
	{
		public bool IsValid { get; } = true;
		public IImmutableList<string> Errors { get; } = ImmutableList<string>.Empty;

		public CommandValidationResult() : this(true) { }

		public CommandValidationResult(bool isValid)
		{
			IsValid = isValid;
		}

		public CommandValidationResult(IList<string> errors)
		{
			IsValid = errors.Count == 0;
			Errors = errors.ToImmutableList();
		}

		ICommandValidationResult FromSuccess() => new CommandValidationResult(true);
		Task<ICommandValidationResult> FromSuccessAsync() => Task.FromResult<ICommandValidationResult>(FromSuccess());

		ICommandValidationResult FromError(string error) => new CommandValidationResult(new List<string>() {error});
		Task<ICommandValidationResult> FromErrorAsync(string error) => Task.FromResult<ICommandValidationResult>(FromError(error));

		ICommandValidationResult FromErrors(IList<string> errors) => new CommandValidationResult(errors);
		Task<ICommandValidationResult> FromErrorsAsync(IList<string> errors) => Task.FromResult<ICommandValidationResult>(FromErrors(errors));
	}
}
