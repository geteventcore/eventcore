using EventCore.EventSourcing;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public class CommandResult : ICommandResult
	{
		public bool IsSuccess { get; }
		public IImmutableList<string> Errors { get; }
		public IImmutableList<IBusinessEvent> Events { get; }

		public CommandResult() : this(new List<IBusinessEvent>()) { }

		public CommandResult(IList<string> errors)
		{
			IsSuccess = false;
			Errors = errors.ToImmutableList();
		}

		public CommandResult(IList<IBusinessEvent> events)
		{
			IsSuccess = true;
			Events = events.ToImmutableList();
		}

		public static CommandResult FromError(string error) => new CommandResult(new List<string>() { error });
		public static Task<CommandResult> FromErrorAsync(string error) => Task.FromResult<CommandResult>(FromError(error));
		public static Task<ICommandResult> FromErrorIAsync(string error) => Task.FromResult<ICommandResult>(FromError(error));

		public static CommandResult FromErrors(IList<string> errors) => new CommandResult(errors);
		public static Task<CommandResult> FromErrorsAsync(IList<string> errors) => Task.FromResult<CommandResult>(FromErrors(errors));
		public static Task<ICommandResult> FromErrorsIAsync(IList<string> errors) => Task.FromResult<ICommandResult>(FromErrors(errors));


		public static CommandResult FromEvent(IBusinessEvent e) => new CommandResult(new List<IBusinessEvent>() { e });
		public static Task<CommandResult> FromEventAsync(IBusinessEvent e) => Task.FromResult<CommandResult>(FromEvent(e));
		public static Task<ICommandResult> FromEventIAsync(IBusinessEvent e) => Task.FromResult<ICommandResult>(FromEvent(e));

		public static CommandResult FromEvents(IList<IBusinessEvent> events) => new CommandResult(events);
		public static Task<CommandResult> FromEventsAsync(IList<IBusinessEvent> events) => Task.FromResult<CommandResult>(FromEvents(events));
		public static Task<ICommandResult> FromEventsIAsync(IList<IBusinessEvent> events) => Task.FromResult<ICommandResult>(FromEvents(events));
	}
}
