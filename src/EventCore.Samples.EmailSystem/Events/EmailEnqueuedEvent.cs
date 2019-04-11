using EventCore.EventSourcing;
using System;

namespace EventCore.Samples.EmailSystem.Events
{
	public class EmailEnqueuedEvent : BusinessEvent
	{
		public readonly Guid EmailId;

		public EmailEnqueuedEvent(BusinessEventMetadata metadata, Guid emailId) : base(metadata)
		{
			EmailId = emailId;
		}
	}
}
