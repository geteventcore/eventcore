using EventCore.EventSourcing;
using EventCore.Samples.SimpleEventStore.EventStoreDb;
using EventCore.Utilities;
using System;
using System.Collections.Generic;

namespace EventCore.Samples.SimpleEventStore.Client
{
	public class StreamClientFactory : IStreamClientFactory
	{
		private readonly IStandardLogger _logger;
		private readonly IDictionary<string, Func<EventStoreDbContext>> _dbFactories;
		private readonly string _notificationsHubUrl;

		public StreamClientFactory(IStandardLogger logger, IDictionary<string, Func<EventStoreDbContext>> dbFactories, string notificationsHubUrl)
		{
			_logger = logger;
			_dbFactories = dbFactories;
			_notificationsHubUrl = notificationsHubUrl;
		}

		public IStreamClient Create(string regionId) => new StreamClient(_logger, _dbFactories[regionId](), _notificationsHubUrl);
	}
}
