using EventStore.ClientAPI;

namespace EventCore.EventSourcing.EventStore
{
	public interface IEventStoreConnectionFactory
	{
		IEventStoreConnection Create(string regionId);
	}
}
