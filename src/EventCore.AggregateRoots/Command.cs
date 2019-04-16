using System;

namespace EventCore.AggregateRoots
{
	public abstract class Command : ICommand
	{
		public ICommandMetadata _Metadata { get; }

		public Command(ICommandMetadata _metadata)
		{
			_Metadata = _metadata;
		}

		public abstract string GetRegionId();
		public abstract string GetAggregateRootId();

		public virtual ICommandValidationResult ValidateSemantics()
		{
			return CommandValidationResult.FromValid(); // TODO: Implement validation attributes...
		}
	}
}
