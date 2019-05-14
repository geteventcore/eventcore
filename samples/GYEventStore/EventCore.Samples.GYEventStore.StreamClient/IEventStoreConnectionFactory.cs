using EventStore.ClientAPI;

namespace EventCore.Samples.GYEventStore.StreamClient
{
	public interface IEventStoreConnectionFactory
	{
		IEventStoreConnection Create(string regionId);
	}
}
