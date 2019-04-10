using EventCore.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulEventSubscriber
{
	public class HandlingManagerHandlerRunner : IHandlingManagerHandlerRunner
	{
		private readonly IStandardLogger _logger;
		private readonly IHandlingManagerAwaiter _awaiter;
		private readonly IStreamStateRepo _streamStateRepo;
		private readonly ISubscriberEventHandler _handler;

		public HandlingManagerHandlerRunner(IStandardLogger logger, IHandlingManagerAwaiter awaiter, IStreamStateRepo streamStateRepo, ISubscriberEventHandler handler)
		{
			_logger = logger;
			_awaiter = awaiter;
			_streamStateRepo = streamStateRepo;
			_handler = handler;
		}

		public async Task TryRunHandlerAsync(SubscriberEvent subscriberEvent, CancellationToken cancellationToken)
		{
			try
			{
				try
				{
					await _handler.HandleAsync(subscriberEvent, cancellationToken);
					await _streamStateRepo.SaveStreamStateAsync(subscriberEvent.StreamId, subscriberEvent.Position, false);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Exception while handling event. Stream will be halted.");

					// Save errored stream state.
					await _streamStateRepo.SaveStreamStateAsync(subscriberEvent.StreamId, subscriberEvent.Position, true);
				}
			}
			finally
			{
				_awaiter.ReleaseThrottle(); // Make room for other handlers.
				_awaiter.SetHandlerCompletionSignal();
			}
		}
	}
}
