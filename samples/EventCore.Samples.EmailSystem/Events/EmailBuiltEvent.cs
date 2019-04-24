using EventCore.EventSourcing;
using System;

namespace EventCore.Samples.EmailSystem.Events
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
