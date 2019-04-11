using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.EmailSystem.Domain.EmailQueue.StateModels
{
	public class EmailQueueMessage
	{
		public readonly Guid EmailId;
		
		public EmailQueueMessage(Guid emailId)
		{
			EmailId = emailId;
		}
	}
}
