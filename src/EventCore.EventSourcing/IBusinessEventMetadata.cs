using System;

namespace EventCore.EventSourcing
{
	public interface IBusinessEventMetadata
	{
		string CausalId { get; }
		string CorrelationId { get; }
		DateTime TimestampUtc { get; }
	}
}
