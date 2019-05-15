using System;

namespace EventCore.AggregateRoots
{
	public abstract class Command : ICommand
	{
		// Use of underscore breaks naming conventions for public members.
		// However, commands are simple DTOs, so we choose to do this as to
		// to not interfere with subclass names.
		public ICommandMetadata _Metadata { get; }

		public Command(CommandMetadata _metadata)
		{
			if (_metadata == null)
			{
				throw new ArgumentNullException("Metadata can't be null!");
			}
			_Metadata = _metadata;
		}

		public virtual string GetCommandId() => _Metadata.CommandId;
		public abstract string GetRegionId();
		public abstract string GetAggregateRootId();

		public virtual ICommandValidationResult ValidateSemantics()
		{
			return CommandValidationResult.FromValid(); // TODO: Implement validation attributes...
		}
	}
}
