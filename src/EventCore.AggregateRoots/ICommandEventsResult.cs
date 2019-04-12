using EventCore.EventSourcing;
using System.Collections.Immutable;

namespace EventCore.AggregateRoots
{
	public interface ICommandEventsResult
	{
		IImmutableList<BusinessEvent> Events { get; }
	}
}
