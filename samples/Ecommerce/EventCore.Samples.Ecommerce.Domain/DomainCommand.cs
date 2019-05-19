using EventCore.AggregateRoots;
using EventCore.Samples.Ecommerce.Shared;
using System;

namespace EventCore.Samples.Ecommerce.Domain
{
	public abstract class DomainCommand : Command
	{
		public DomainCommand(CommandMetadata _metadata) : base(_metadata) { }

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

		// Only one region for this sample. If supporting multiple regions then
		// region would be determined by data given in command, e.g. a customer's location, origin of request IP, etc.
		public override string GetRegionId() => Constants.DEFAULT_REGION_ID;
	}
}
