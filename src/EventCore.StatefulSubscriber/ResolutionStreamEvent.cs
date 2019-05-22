using EventCore.EventSourcing;

namespace EventCore.StatefulSubscriber
{
	public class ResolutionStreamEvent
	{
		public readonly string RegionId;
		public readonly StreamEvent StreamEvent;

		public ResolutionStreamEvent(string regionId, StreamEvent streamEvent)
		{
			RegionId = regionId;
			StreamEvent = streamEvent;
		}
	}
}
