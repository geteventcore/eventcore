namespace EventCore.EventSourcing.EventStore
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
