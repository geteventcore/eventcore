using EventStore.ClientAPI;
using System;
using System.Collections.Generic;

namespace EventCore.EventSourcing.EventStore
{
	public class EventStoreConnectionFactory : IEventStoreConnectionFactory
	{
		IDictionary<string, Func<IEventStoreConnection>> _connectionBuilders;

		public EventStoreConnectionFactory(IDictionary<string, Func<IEventStoreConnection>> connectionBuilders)
		{
			_connectionBuilders = connectionBuilders;
		}

		public IEventStoreConnection Create(string region)
		{
			if (!_connectionBuilders.ContainsKey(region))
				throw new ArgumentException("No connection for given region key.");

			return _connectionBuilders[region]();
		}
	}
}
