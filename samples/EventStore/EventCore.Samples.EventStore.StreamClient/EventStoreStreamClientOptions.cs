﻿namespace EventCore.Samples.EventStore.StreamClient
{
	public class EventStoreStreamClientOptions
	{
		public int StreamReadBatchSize { get; }

		public EventStoreStreamClientOptions(int streamReadBatchSize)
		{
			StreamReadBatchSize = streamReadBatchSize;
		}
	}
}
