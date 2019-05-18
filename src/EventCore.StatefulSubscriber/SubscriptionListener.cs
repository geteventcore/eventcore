using EventCore.EventSourcing;
using EventCore.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public class SubscriptionListener : ISubscriptionListener
	{
		private readonly IStandardLogger _logger;
		private readonly IStreamClientFactory _streamClientFactory;
		private readonly IResolutionManager _resolutionManager;

		public SubscriptionListener(IStandardLogger logger, IStreamClientFactory streamClientFactory, IResolutionManager resolutionManager)
		{
			_logger = logger;
			_streamClientFactory = streamClientFactory;
			_resolutionManager = resolutionManager;
		}

		// Thread safe, will be called in parallel by one caller for multiple regions.
		public async Task ListenAsync(string regionId, string subscriptionStreamId, CancellationToken cancellationToken)
		{
			try
			{
				using (var streamClient = _streamClientFactory.Create(regionId))
				{
					while (!cancellationToken.IsCancellationRequested)
					{
						// Subscription starts from first position in stream.
						// Streams states will be read to skip previously processed events.
						var listenerTask = streamClient.SubscribeToStreamAsync(
							subscriptionStreamId, streamClient.FirstPositionInStream,
							(se) => _resolutionManager.ReceiveStreamEventAsync(se, streamClient.FirstPositionInStream, cancellationToken),
							cancellationToken
						);
						await Task.WhenAny(new[] { listenerTask, cancellationToken.WaitHandle.AsTask() });
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception while listening on regional subscription.");
				throw;
			}
		}
	}
}
