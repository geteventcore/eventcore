using System;
using EventCore.AggregateRoots;

namespace EventCore.Samples.Ecommerce.Domain
{
	public class DomainCommandMetadata
	{
		public string CommandId { get; }
		public DateTime TimestampUtc { get; }

		public DomainCommandMetadata(string commandId)
		{
			CommandId = commandId;
			TimestampUtc = DateTime.UtcNow;
		}
	}
}
