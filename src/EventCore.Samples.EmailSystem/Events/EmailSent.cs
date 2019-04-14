using EventCore.EventSourcing;
using System;

namespace EventCore.Samples.EmailSystem.Events
{
	public class EmailSent : BusinessEvent
	{
		public readonly Guid EmailId;

		public EmailSent(BusinessEventMetadata metadata, Guid emailId) : base(metadata)
		{
			EmailId = emailId;
		}
	}
}
