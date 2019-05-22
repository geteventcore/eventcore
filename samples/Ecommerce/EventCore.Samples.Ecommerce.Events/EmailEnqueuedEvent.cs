using EventCore.EventSourcing;
using System;

namespace EventCore.Samples.Ecommerce.Events
{
	public class EmailEnqueuedEvent : BusinessEvent
	{
		public readonly Guid EmailId;
		public readonly string From;
		public readonly string To;
		public readonly string Subject;
		public readonly string Body;

		public EmailEnqueuedEvent(BusinessEventMetadata _metadata, Guid emailId, string from, string to, string subject, string body) : base(_metadata)
		{
			EmailId = emailId;
			From = from;
			To = to;
			Subject = subject;
			Body = body;
		}
	}
}
