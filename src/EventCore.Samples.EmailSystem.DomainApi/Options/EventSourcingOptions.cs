﻿namespace EventCore.Samples.EmailSystem.DomainApi.Options
{
	public class EventSourcingOptions
	{
		public int StreamReadBatchSize { get; set; } = 100;

		public EventSourcingOptions()
		{
		}
	}
}