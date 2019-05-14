namespace EventCore.Samples.GYEventStore.StreamClient
{
	public class EventStoreStreamClientOptions
	{
		public int StreamReadBatchSize { get; }
		public int ReconnectDelaySeconds { get; } = 5;

		public EventStoreStreamClientOptions(int streamReadBatchSize)
		{
			StreamReadBatchSize = streamReadBatchSize;
		}
	}
}
