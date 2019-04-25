using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.Ecommerce.Domain.EmailBuilder
{
	public abstract class EmailBuilderCommand : DomainCommand
	{
		public override string _AggregateRootName { get => EmailBuilderAggregate.NAME; }

		public readonly Guid EmailId;

		public EmailBuilderCommand(ICommandMetadata _metadata, Guid emailId) : base(_metadata)
		{
			EmailId = emailId;
		}
		
		public override string GetAggregateRootId() => EmailId.ToString();
	}
}
