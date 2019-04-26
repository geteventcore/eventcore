using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.Ecommerce.Domain.EmailQueue
{
	public abstract class EmailQueueCommand : DomainCommand
	{
		public override string _AggregateRootName { get => EmailQueueAggregate.NAME; }

		public readonly Guid EmailId;

		public EmailQueueCommand(ICommandMetadata _metadata, Guid emailId) : base(_metadata)
		{
			EmailId = emailId;
		}
		
		public override string GetAggregateRootId() => EmailId.ToString();
	}
}
