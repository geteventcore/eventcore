using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.EmailSystem.Domain.EmailQueue
{
	public class EmailQueueCommand : Command
	{
		public readonly Guid EmailId;

		public EmailQueueCommand(CommandMetadata metadata, Guid emailId) : base(metadata)
		{
			EmailId = emailId;
		}

		public override string RegionId() => null;
		public override string AggregateRootId() => EmailId.ToString();
	}
}
