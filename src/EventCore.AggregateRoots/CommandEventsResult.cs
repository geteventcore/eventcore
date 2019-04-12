using EventCore.EventSourcing;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots
{
	public class CommandEventsResult : ICommandEventsResult
	{
		public IImmutableList<BusinessEvent> Events { get; }

		public CommandEventsResult() : this(new List<BusinessEvent>())
		{
		}

		public CommandEventsResult(IList<BusinessEvent> events)
		{
			Events = events.ToImmutableList();
		}

		public static ICommandEventsResult FromEvent(BusinessEvent e) => new CommandEventsResult(new List<BusinessEvent>() { e });
		public static Task<ICommandEventsResult> FromEventAsync(BusinessEvent e) => Task.FromResult<ICommandEventsResult>(FromEvent(e));

		public static ICommandEventsResult FromEvents(IList<BusinessEvent> events) => new CommandEventsResult(events);
		public static Task<ICommandEventsResult> FromEventsAsync(IList<BusinessEvent> events) => Task.FromResult<ICommandEventsResult>(FromEvents(events));
	}
}
