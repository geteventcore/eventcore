namespace EventCore.StatefulEventSubscriber
{
	public class SubscriberOptions
	{
		public readonly int MaxParallelHandlerExecutions;
		public readonly string StreamId;
		public readonly string[] RegionIds;

		public SubscriberOptions(int maxParallelHandlerExecutions, string streamId, string[] regionIds)
		{
			MaxParallelHandlerExecutions = maxParallelHandlerExecutions;
			StreamId = streamId;
			RegionIds = regionIds;
		}
	}
}
