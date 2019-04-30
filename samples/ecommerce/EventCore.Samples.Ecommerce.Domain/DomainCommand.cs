using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.Ecommerce.Domain
{
	public abstract class DomainCommand : Command
	{
		// Use of underscore breaks naming conventions for public fields but since commands are
		// simple data transfer objects we want to clearly separate which properties/methods are
		// for the system and which are for the business properties in a concrete command.

		public DomainCommandMetadata _Metadata { get; }

		public string GetCommandName()
		{
			var commandName = this.GetType().Name;
			var removeString = "Command";
			int lastIndex = commandName.LastIndexOf(removeString, StringComparison.OrdinalIgnoreCase);
			return (lastIndex < 0)
					? commandName
					: commandName.Remove(lastIndex, removeString.Length);
		}

		public abstract string GetAggregateRootName();
		public override string GetCommandId() => _Metadata.CommandId;
		public override string GetRegionId() => Constants.DEFAULT_REGION_ID; // Only one region for now.

		public DomainCommand(DomainCommandMetadata _metadata)
		{
			_Metadata = _metadata;
			if(_metadata == null) throw new ArgumentNullException("Metadata is null!");
		}
	}
}
