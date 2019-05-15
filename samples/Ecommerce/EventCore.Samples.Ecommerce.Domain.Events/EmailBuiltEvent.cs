using EventCore.EventSourcing;
using System;

namespace EventCore.Samples.Ecommerce.Domain.Events
{
	public class EmailBuiltEvent : BusinessEvent
	{
		public readonly Guid EmailId;

		public EmailBuiltEvent(BusinessEventMetadata _metadata, Guid emailId) : base(_metadata)
		{
			EmailId = emailId;
		}
	}
}
