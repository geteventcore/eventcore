using EventCore.EventSourcing;
using System;

namespace EventCore.Samples.Ecommerce.Domain.Events
{
	public class EmailSendFailed : BusinessEvent
	{
		public readonly Guid EmailId;
		public readonly string ErrorMessage;

		public EmailSendFailed(BusinessEventMetadata metadata, Guid emailId, string errorMessage) : base(metadata)
		{
			EmailId = emailId;
			ErrorMessage = errorMessage;
		}
	}
}
