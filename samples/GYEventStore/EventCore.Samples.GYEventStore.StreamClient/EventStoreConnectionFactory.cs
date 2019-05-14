using EventStore.ClientAPI;
using System;
using System.Collections.Generic;

namespace EventCore.Samples.GYEventStore.StreamClient
{
	public class EventStoreConnectionFactory : IEventStoreConnectionFactory
	{
		IDictionary<string, Func<IEventStoreConnection>> _connectionBuilders;

		public EventStoreConnectionFactory(IDictionary<string, Func<IEventStoreConnection>> connectionBuilders)
		{
			_connectionBuilders = connectionBuilders;
		}

		public IEventStoreConnection Create(string regionId)
		{
			if (!_connectionBuilders.ContainsKey(regionId))
				throw new ArgumentException("No connection for given region id.");

			return _connectionBuilders[regionId]();
		}
	}
}
