using System;

namespace EventCore.EventSourcing
{
	public class BusinessEvent
	{
		public BusinessMetadata Metadata { get; }

		public BusinessEvent(BusinessMetadata metadata)
		{
			Metadata = metadata;
		}
	}
}
