using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.Ecommerce.Domain.EmailQueue.StateModels
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
