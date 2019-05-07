namespace EventCore.Samples.EventStore.StreamDb.DbModels
{
	public class StreamEventDbModel
	{
		public long GlobalPosition { get; set; } // Global index for all events.
		public string StreamId { get; set; }
		public long StreamPosition { get; set; }
		public string EventType { get; set; }
		public byte[] EventData { get; set; }
	}
}
