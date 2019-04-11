using System;

namespace EventCore.EventSourcing
{
	public class BusinessEventMetadata
	{
		public readonly string CausalId;
		public readonly string CorrelationId;
		public readonly DateTime TimestampUtc;

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
