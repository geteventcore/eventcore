using System;

namespace EventCore.EventSourcing
{
	public abstract class BusinessEvent : IBusinessEvent
	{
		public IBusinessEventMetadata Metadata { get; }

		public BusinessEvent(IBusinessEventMetadata metadata)
		{
			Metadata = metadata;
		}
	}
}