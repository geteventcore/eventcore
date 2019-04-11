using System;

namespace EventCore.AggregateRoots
{
	public class CommandMetadata
	{
		public readonly string CommandId;
		public readonly DateTime TimestampUtc;

		public CommandMetadata(string commandId)
		{
			CommandId = commandId;
			TimestampUtc = DateTime.UtcNow;
		}

		public static CommandMetadata Empty { get => new CommandMetadata(null); }
	}
}
