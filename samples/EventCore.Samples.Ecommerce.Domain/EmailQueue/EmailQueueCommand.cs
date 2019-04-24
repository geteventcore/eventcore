using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.Ecommerce.Domain.EmailQueue
{
	public abstract class EmailQueueCommand : DomainCommand
	{
		public override string _AggregateRootName { get => EmailQueueAggregate.NAME; }

		public readonly Guid EmailId;

		public EmailQueueCommand(CommandMetadata metadata, Guid emailId) : base(metadata)
		{
			EmailId = emailId;
		}

		public override string GetRegionId() => null;
		public override string GetAggregateRootId() => EmailId.ToString();
	}
}
