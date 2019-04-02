using System;

namespace EventCore.EventSourcing
{
	public class BusinessEvent
	{
		public string CausalId { get; }
		public string CorrelationId { get; }

		public BusinessEvent(string causalId, string correlationId)
		{
			CausalId = causalId;
			CorrelationId = correlationId;
		}
	}
}
