using EventCore.EventSourcing;
using EventCore.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.EventSourcing.StatefulSubscriber
{
	public class SubscriptionListener : ISubscriptionListener
	{
		private readonly IStandardLogger _logger;
		private readonly IStreamClient _streamClient;
		private readonly IResolutionManager _resolutionManager;

		public SubscriptionListener(IStandardLogger logger, IStreamClient streamClient, IResolutionManager resolutionManager)
		{
			_logger = logger;
			_streamClient = streamClient;
			_resolutionManager = resolutionManager;
		}

		// Thread safe, will be called in parallel by one caller for multiple regions.
		public async Task ListenAsync(string regionId, string subscriptionStreamId, CancellationToken cancellationToken)
		{
			try
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					// Subscription starts from first position in stream.
					// Streams states will be read to skip previously processed events.
					var listenerTask = _streamClient.SubscribeToStreamAsync(
						regionId, subscriptionStreamId, _streamClient.FirstPositionInStream,
						(se, ct) => _resolutionManager.ReceiveStreamEventAsync(se, _streamClient.FirstPositionInStream, ct),
						cancellationToken
					);
					await Task.WhenAny(new[] { listenerTask, cancellationToken.WaitHandle.AsTask() });
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
