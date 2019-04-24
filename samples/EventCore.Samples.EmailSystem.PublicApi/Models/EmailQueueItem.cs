using System;

namespace EventCore.Samples.EmailSystem.PublicApi.Models
{
	public class EmailQueueItem
	{
		public readonly Guid EmailId;
		public readonly string FromAddress;
		public readonly string ToAddress;
		public readonly string Subject;
		public readonly string Body;
		public readonly bool IsHtml;

		public EmailQueueItem(Guid emailId, string fromAddress, string toAddress, string subject, string body, bool isHtml)
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
