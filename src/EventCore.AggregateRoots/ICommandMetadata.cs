using System;

namespace EventCore.AggregateRoots
{
	public interface ICommandMetadata
	{
		string CommandId { get; }
		DateTime TimestampUtc { get; }
	}
}
