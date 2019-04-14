using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.EmailSystem.Domain
{
	public abstract class DomainCommand : Command
	{
		// Use of underscore breaks naming conventions but this is special case
		// to avoid naming collisions with subclasses.
		public string _CommandName
		{
			get
			{
				var commandName = this.GetType().Name;
				var removeString = "Command";
				int lastIndex = commandName.LastIndexOf(removeString, StringComparison.OrdinalIgnoreCase);
				return (lastIndex < 0)
						? commandName
						: commandName.Remove(lastIndex, removeString.Length);
			}
		}

		public abstract string _AggregateRootName { get; }

		public DomainCommand(CommandMetadata metadata) : base(metadata)
		{
		}
	}
}
