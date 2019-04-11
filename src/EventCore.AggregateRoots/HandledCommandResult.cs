using EventCore.EventSourcing;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public class HandledCommandResult : IHandledCommandResult
	{
		public bool IsSuccess { get; } = true;
		public IImmutableList<string> Errors { get; } = ImmutableList<string>.Empty;
		public IImmutableList<BusinessEvent> Events { get; } = ImmutableList<BusinessEvent>.Empty;

		public HandledCommandResult(IList<string> errors)
		{
			IsSuccess = false;
			Errors = errors.ToImmutableList();
		}

		public HandledCommandResult(IList<BusinessEvent> events)
		{
			Events = events.ToImmutableList();
		}

		public static IHandledCommandResult FromError(string error) => new HandledCommandResult(new List<string>() { error });
		public static Task<IHandledCommandResult> FromErrorAsync(string error) => Task.FromResult<IHandledCommandResult>(FromError(error));

		public static IHandledCommandResult FromErrors(IList<string> errors) => new HandledCommandResult(errors);
		public static Task<IHandledCommandResult> FromErrorsAsync(IList<string> errors) => Task.FromResult<IHandledCommandResult>(FromErrors(errors));

		public static IHandledCommandResult FromEvent(BusinessEvent e) => new HandledCommandResult(new List<BusinessEvent>() { e });
		public static Task<IHandledCommandResult> FromEventAsync(BusinessEvent e) => Task.FromResult<IHandledCommandResult>(FromEvent(e));

		public static IHandledCommandResult FromEvents(IList<BusinessEvent> events) => new HandledCommandResult(events);
		public static Task<IHandledCommandResult> FromEventsAsync(IList<BusinessEvent> events) => Task.FromResult<IHandledCommandResult>(FromEvents(events));
	}
}
