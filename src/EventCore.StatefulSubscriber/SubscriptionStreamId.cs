namespace EventCore.StatefulSubscriber
{
	public class SubscriptionStreamId
	{
		public readonly string RegionId;
		public readonly string StreamId;

		public SubscriptionStreamId(string regionId, string streamId)
		{
			RegionId = regionId;
			StreamId = streamId;
		}
	}
}
