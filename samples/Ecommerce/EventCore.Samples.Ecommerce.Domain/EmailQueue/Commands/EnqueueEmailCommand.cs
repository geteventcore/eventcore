using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.Ecommerce.Domain.EmailQueue.Commands
{
	public class EnqueueEmailCommand : EmailQueueCommand
	{
		public readonly string From;
		public readonly string To;
		public readonly string Subject;
		public readonly string Body;
		public readonly bool IsHtml;

		public EnqueueEmailCommand(CommandMetadata _metadata, Guid emailId, string fromAddress, string toAddress, string subject, string body, bool isHtml) : base(_metadata, emailId)
		{
			From = fromAddress;
			To = toAddress;
			Subject = subject;
			Body = body;
			IsHtml = isHtml;
		}
	}
}
