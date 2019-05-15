using System;

namespace EventCore.AggregateRoots
{
	public class CommandMetadata : ICommandMetadata
	{
		public string CommandId { get; }
		public DateTime TimestampUtc { get; }

		public CommandMetadata(string commandId, DateTime? timestampUtc = null)
		{
			CommandId = commandId;
			TimestampUtc = timestampUtc ?? DateTime.UtcNow;
		}

		public static CommandMetadata Default { get => new CommandMetadata(Guid.NewGuid().ToString()); }
		public static CommandMetadata FromCommandId(string commandId) => new CommandMetadata(commandId);
	}
}
