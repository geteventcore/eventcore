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

		public static ICommandEventsResult FromEvent(IBusinessEvent e) => new CommandEventsResult(new List<IBusinessEvent>() { e });
		public static Task<ICommandEventsResult> FromEventAsync(IBusinessEvent e) => Task.FromResult<ICommandEventsResult>(FromEvent(e));

		public static ICommandEventsResult FromEvents(IList<IBusinessEvent> events) => new CommandEventsResult(events);
		public static Task<ICommandEventsResult> FromEventsAsync(IList<IBusinessEvent> events) => Task.FromResult<ICommandEventsResult>(FromEvents(events));
	}
}
