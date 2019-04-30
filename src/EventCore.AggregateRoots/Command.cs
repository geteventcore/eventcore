using System;

namespace EventCore.AggregateRoots
{
	public abstract class Command : ICommand
	{
		public abstract string GetCommandId();
		public abstract string GetRegionId();
		public abstract string GetAggregateRootId();

		public virtual ICommandValidationResult ValidateSemantics()
		{
			return CommandValidationResult.FromValid(); // TODO: Implement validation attributes...
		}
	}
}
