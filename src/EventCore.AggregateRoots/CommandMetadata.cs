using EventCore.AggregateRoots;
using System;

namespace EventCore.AggregateRoots
{
	public class CommandMetadata : ICommandMetadata
	{
		public string CommandId { get; }
		public DateTime TimestampUtc { get; }

		public CommandMetadata(string commandId)
		{
			CommandId = commandId;
			TimestampUtc = DateTime.UtcNow;
		}

		public static CommandMetadata Empty { get => new CommandMetadata(null); }
	}
}
