using EventCore.EventSourcing;
using System;

namespace EventCore.Samples.Ecommerce.Domain.Events
{
	public class EmailBuiltEvent : BusinessEvent
	{
		public readonly Guid EmailId;

		public EmailBuiltEvent(BusinessEventMetadata metadata, Guid emailId) : base(metadata)
		{
			EmailId = emailId;
		}
	}
}
