using EventCore.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.EventSourcing
{
	public class StatefulSubscriber : IStatefulSubscriber
	{
		private readonly IGenericLogger _logger;
		private readonly IStreamClient _streamClient;

		public StatefulSubscriber(IGenericLogger logger, IStreamClient streamClient)
		{
			_logger = logger;
			_streamClient = streamClient;
		}

		public Task SubscribeAsync(string streamId, long fromPosition, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
