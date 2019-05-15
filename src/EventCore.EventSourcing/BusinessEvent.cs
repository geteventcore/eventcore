using System;

namespace EventCore.EventSourcing
{
	public abstract class BusinessEvent : BusinessEvent<BusinessEventMetadata>
	{
		public BusinessEvent(BusinessEventMetadata _metadata) : base(_metadata) { }
	}

	public abstract class BusinessEvent<TMetadata> : IBusinessEvent where TMetadata : IBusinessEventMetadata
	{
		// Use of underscore breaks naming conventions for public members.
		// However, commands are simple DTOs, so we choose to do this as to
		// to not interfere with subclass names.
		public TMetadata _Metadata { get; }

		public BusinessEvent(TMetadata _metadata)
		{
			_Metadata = _metadata;
		}

		public string GetCausalId() => _Metadata.CausalId;
		public string GetCorrelationId() => _Metadata.CorrelationId;
	}
}