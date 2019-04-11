using System;

namespace EventCore.EventSourcing
{
	public class BusinessEvent
	{
		public readonly BusinessEventMetadata Metadata;

		public BusinessEvent(BusinessEventMetadata metadata)
		{
			Metadata = metadata;
		}
	}
}
