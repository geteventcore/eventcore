using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.Ecommerce.Domain.EmailBuilder
{
	public abstract class EmailBuilderCommand : DomainCommand
	{
		public readonly Guid EmailId;

		public EmailBuilderCommand(DomainCommandMetadata _metadata, Guid emailId) : base(_metadata)
		{
			EmailId = emailId;
		}
		
		public override string GetAggregateRootName() => EmailBuilderAggregate.NAME;
		public override string GetAggregateRootId() => EmailId.ToString();
	}
}
