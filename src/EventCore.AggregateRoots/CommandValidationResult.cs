using EventCore.EventSourcing;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public class CommandValidationResult : ICommandValidationResult
	{
		public bool IsValid { get; } = true;
		public IImmutableList<string> Errors { get; }

		public CommandValidationResult() : this(new List<string>()) { }

		public CommandValidationResult(IList<string> errors)
		{
			IsValid = errors.Count == 0;
			Errors = errors.ToImmutableList();
		}

		public static CommandValidationResult FromValid() => new CommandValidationResult();
		public static Task<CommandValidationResult> FromValidAsync() => Task.FromResult<CommandValidationResult>(FromValid());
		public static Task<ICommandValidationResult> FromValidIAsync() => Task.FromResult<ICommandValidationResult>(FromValid());

		public static CommandValidationResult FromError(string error) => new CommandValidationResult(new List<string>() {error});
		public static Task<CommandValidationResult> FromErrorAsync(string error) => Task.FromResult<CommandValidationResult>(FromError(error));
		public static Task<ICommandValidationResult> FromErrorIAsync(string error) => Task.FromResult<ICommandValidationResult>(FromError(error));

		public static CommandValidationResult FromErrors(IList<string> errors) => new CommandValidationResult(errors);
		public static Task<CommandValidationResult> FromErrorsAsync(IList<string> errors) => Task.FromResult<CommandValidationResult>(FromErrors(errors));
		public static Task<ICommandValidationResult> FromErrorsIAsync(IList<string> errors) => Task.FromResult<ICommandValidationResult>(FromErrors(errors));
	}
}
