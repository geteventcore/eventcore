using EventCore.EventSourcing;
using System.Collections.Immutable;

namespace EventCore.AggregateRoots
{
	public interface IHandledCommandResult
	{
		bool IsSuccess { get; }
		IImmutableList<string> Errors { get; }
		IImmutableList<BusinessEvent> Events { get; }
	}
}
