using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.EmailSystem.Domain.EmailQueue.StateModels
{
	public class EmailQueueMessageModel
	{
		public readonly Guid EmailId;
		
		public EmailQueueMessageModel(Guid emailId)
		{
			EmailId = emailId;
		}
	}
}
