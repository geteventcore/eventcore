using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.Ecommerce.Domain.EmailQueue
{
	public abstract class EmailQueueCommand : DomainCommand
	{
		public readonly Guid EmailId;

		public EmailQueueCommand(DomainCommandMetadata _metadata, Guid emailId) : base(_metadata)
		{
			EmailId = emailId;
		}

		public override string GetAggregateRootName() => EmailQueueAggregate.NAME;
		public override string GetAggregateRootId() => EmailId.ToString();
	}
}
