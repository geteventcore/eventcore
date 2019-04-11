using System.Collections.Immutable;

namespace EventCore.AggregateRoots
{
	public interface ICommandValidationResult
	{
		bool IsValid { get; }
		IImmutableList<string> Errors { get; }
	}
}
