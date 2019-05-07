namespace EventCore.Samples.EventStore.StreamDb.DbModels
{
	public class StreamEventDbModel
	{
		public long GlobalId { get; set; } // Global index for all events.
		public string StreamId { get; set; }
		public long EventNumber { get; set; }
		public byte[] EventData { get; set; }
	}
}
