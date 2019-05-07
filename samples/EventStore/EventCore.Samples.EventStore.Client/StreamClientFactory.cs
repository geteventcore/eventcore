using EventCore.EventSourcing;
using EventCore.Samples.EventStore.StreamDb;
using EventCore.Utilities;
using System;
using System.Collections.Generic;

namespace EventCore.Samples.EventStore.Client
{
	public class StreamClientFactory : IStreamClientFactory
	{
		private readonly IStandardLogger _logger;
		private readonly IDictionary<string, Func<StreamDbContext>> _dbFactories;

		public StreamClientFactory(IStandardLogger logger, IDictionary<string, Func<StreamDbContext>> dbFactories)
		{
			_logger = logger;
			_dbFactories = dbFactories;
		}

		public IStreamClient Create(string regionId) => new StreamClient(_logger, _dbFactories[regionId]());
	}
}
