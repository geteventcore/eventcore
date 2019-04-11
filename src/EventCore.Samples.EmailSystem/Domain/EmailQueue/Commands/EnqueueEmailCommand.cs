using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.EmailSystem.Domain.EmailQueue.Commands
{
	public class EnqueueEmailCommand : EmailQueueCommand
	{
		public readonly string FromAddress;
		public readonly string ToAddress;
		public readonly string Subject;
		public readonly string Body;
		public readonly bool IsHtml;

		public EnqueueEmailCommand(CommandMetadata metadata, Guid emailId, string fromAddress, string toAddress, string subject, string body, bool isHtml) : base(metadata, emailId)
		{
			FromAddress = fromAddress;
			ToAddress = toAddress;
			Subject = subject;
			Body = body;
			IsHtml = isHtml;
		}
	}
}
