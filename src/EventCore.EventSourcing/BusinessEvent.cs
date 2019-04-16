using System;

namespace EventCore.EventSourcing
{
	public abstract class BusinessEvent
	{
		public readonly BusinessEventMetadata Metadata;

		public BusinessEvent(BusinessEventMetadata metadata)
		{
			Metadata = metadata;
		}
	}
}
