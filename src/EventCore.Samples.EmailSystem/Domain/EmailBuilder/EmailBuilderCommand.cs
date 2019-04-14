using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.EmailSystem.Domain.EmailBuilder
{
	public abstract class EmailBuilderCommand : DomainCommand
	{
		public readonly Guid EmailId;

		public EmailBuilderCommand(CommandMetadata metadata, Guid emailId) : base(metadata)
		{
			EmailId = emailId;
		}

		public override string RegionId() => null;
		public override string AggregateRootId() => EmailId.ToString();
	}
}
