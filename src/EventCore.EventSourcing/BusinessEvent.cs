using System;

namespace EventCore.EventSourcing
{
	public class BusinessEvent
	{
		public readonly BusinessMetadata Metadata;

		public BusinessEvent(BusinessMetadata metadata)
		{
			Metadata = metadata;
		}
	}
}
