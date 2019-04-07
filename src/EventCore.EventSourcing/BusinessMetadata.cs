using System;

namespace EventCore.EventSourcing
{
	public class BusinessMetadata
	{
		public readonly string CausalId;
		public readonly string CorrelationId;

		public BusinessMetadata(string causalId, string correlationId)
		{
			CausalId = causalId;
			CorrelationId = correlationId;
		}

		public static BusinessMetadata Empty { get => new BusinessMetadata(null, null); }
		public static BusinessMetadata Create(string causalId, string correlationId) => new BusinessMetadata(causalId, correlationId);
	}
}
