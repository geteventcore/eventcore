using System;

namespace EventCore.EventSourcing
{
	public class BusinessEventMetadata : IBusinessEventMetadata
	{
		public string CausalId { get; }
		public string CorrelationId { get; }
		public DateTime TimestampUtc { get; }

		public BusinessEventMetadata(string causalId, string correlationId)
		{
			CausalId = causalId;
			CorrelationId = correlationId;
			TimestampUtc = DateTime.UtcNow;
		}

		public static BusinessEventMetadata Empty { get => new BusinessEventMetadata(null, null); }
		public static BusinessEventMetadata FromCausalId(string causalId) => new BusinessEventMetadata(causalId, null);
	}
}
