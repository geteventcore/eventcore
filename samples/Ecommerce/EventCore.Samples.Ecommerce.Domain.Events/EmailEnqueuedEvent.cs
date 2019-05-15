using EventCore.EventSourcing;
using System;

namespace EventCore.Samples.Ecommerce.Domain.Events
{
	public class EmailEnqueuedEvent : BusinessEvent
	{
		public readonly Guid EmailId;
		public readonly string FromAddress;
		public readonly string ToAddress;
		public readonly string Subject;
		public readonly string Body;
		public readonly bool IsHtml;

		public EmailEnqueuedEvent(BusinessEventMetadata _metadata, Guid emailId, string fromAddress, string toAddress, string subject, string body, bool isHtml) : base(_metadata)
		{
			EmailId = emailId;
			FromAddress = fromAddress;
			ToAddress = toAddress;
			Subject = subject;
			Body = body;
			IsHtml = isHtml;
		}
	}
}
