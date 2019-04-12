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

		public static ICommandValidationResult FromValid() => new CommandValidationResult();
		public static Task<ICommandValidationResult> FromValidAsync() => Task.FromResult<ICommandValidationResult>(FromValid());

		public static ICommandValidationResult FromError(string error) => new CommandValidationResult(new List<string>() {error});
		public static Task<ICommandValidationResult> FromErrorAsync(string error) => Task.FromResult<ICommandValidationResult>(FromError(error));

		public static ICommandValidationResult FromErrors(IList<string> errors) => new CommandValidationResult(errors);
		public static Task<ICommandValidationResult> FromErrorsAsync(IList<string> errors) => Task.FromResult<ICommandValidationResult>(FromErrors(errors));
	}
}
