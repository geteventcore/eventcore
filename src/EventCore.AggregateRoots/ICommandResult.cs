using EventCore.EventSourcing;
using System.Collections.Immutable;

namespace EventCore.AggregateRoots
{
	public interface ICommandResult
	{
		bool IsSuccess { get; }
		IImmutableList<string> Errors { get; }
		IImmutableList<IBusinessEvent> Events { get; }
	}
}
