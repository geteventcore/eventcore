using EventCore.EventSourcing;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public class CommandEventsResult : ICommandEventsResult
	{
		public IImmutableList<IBusinessEvent> Events { get; }

		public CommandEventsResult() : this(new List<IBusinessEvent>())
		{
		}

		public CommandEventsResult(IList<IBusinessEvent> events)
		{
			Events = events.ToImmutableList();
		}

		public static CommandEventsResult FromEvent(IBusinessEvent e) => new CommandEventsResult(new List<IBusinessEvent>() { e });
		public static Task<CommandEventsResult> FromEventAsync(IBusinessEvent e) => Task.FromResult<CommandEventsResult>(FromEvent(e));
		public static Task<ICommandEventsResult> FromEventIAsync(IBusinessEvent e) => Task.FromResult<ICommandEventsResult>(FromEvent(e));

		public static CommandEventsResult FromEvents(IList<IBusinessEvent> events) => new CommandEventsResult(events);
		public static Task<CommandEventsResult> FromEventsAsync(IList<IBusinessEvent> events) => Task.FromResult<CommandEventsResult>(FromEvents(events));
		public static Task<ICommandEventsResult> FromEventsIAsync(IList<IBusinessEvent> events) => Task.FromResult<ICommandEventsResult>(FromEvents(events));
	}
}
