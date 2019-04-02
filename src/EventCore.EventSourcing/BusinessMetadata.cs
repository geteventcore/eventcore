using System;

namespace EventCore.EventSourcing
{
	public class BusinessMetadata
	{
		public string CausalId { get; }
		public string CorrelationId { get; }

		public BusinessMetadata(string causalId, string correlationId)
		{
			CausalId = causalId;
			CorrelationId = correlationId;
		}

		public static BusinessMetadata Empty { get => new BusinessMetadata(null, null); }
		public static BusinessMetadata Create(string causalId, string correlationId) => new BusinessMetadata(causalId, correlationId);
	}
}
