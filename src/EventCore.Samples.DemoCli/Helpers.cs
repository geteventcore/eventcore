using CommandLine;
using EventCore.EventSourcing.EventStore;
using System;
using System.Collections.Generic;

namespace EventCore.Samples.DemoCli
{
	public static class Helpers
	{
		public static IEventStoreConnectionFactory EventStoreConnectionFactory
		{
			get
			{
				var builders = new Dictionary<string, Func<EventStore.ClientAPI.IEventStoreConnection>>(StringComparer.OrdinalIgnoreCase);
				builders.Add(Constants.EVENTSTORE_DEFAULT_REGION, () => EventStore.ClientAPI.EventStoreConnection.Create(Constants.EVENTSTORE_URI_CONN_STR));
				return new EventStoreConnectionFactory(builders);
			}
		}
	}
}
