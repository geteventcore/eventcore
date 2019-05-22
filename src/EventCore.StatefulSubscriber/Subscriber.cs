using EventCore.EventSourcing;
using EventCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public class Subscriber : ISubscriber
	{
		// General flow of execution...
		// 1. Stream events received from stream client listening on multiple regions.
		// 2. Stream events enqueued for deserialization/resolution.
		// 3. Deserialization attempted resulting in subscriber events.
		// 4. Subscriber events enqueued for sorting into parallel handler executions.
		// 5. Subscriber events sorted into handling queues grouped by parallel key.
		// 6. Handlers called in parallel by pulling events in order (per queue) off of handling queues.
		//
		// Handling queues are identified by parallel keys, where each parallel queue has guaranteed
		// in-order events per stream. Multiple streams may be interleaved into one parallel queue,
		// and all events from a specific stream will be in order in any given parallel queue, but there is no
		// temporal guarantee across multiple parallel queues. Therefore, events in a stream may be handled out of
		// order between multiple parallel handlers, so it is up to the client implementation to sort events
		// into parallel keys with this consideration.

		private readonly IStandardLogger _logger;
		private readonly ISubscriptionListener _subscriptionListener;
		private readonly IStreamClientFactory _streamClientFactory;
		private readonly IResolutionManager _resolutionManager;
		private readonly ISortingManager _sortingManager;
		private readonly IHandlingManager _handlingManager;
		private readonly IStreamStateRepo _streamStateRepo;
		private readonly SubscriberOptions _options;

		private bool _isSubscribing = false;

		public Subscriber(
			IStandardLogger logger, IStreamClientFactory streamClientFactory, ISubscriptionListener subscriptionListener,
			IResolutionManager resolutionManager, ISortingManager sortingManager, IHandlingManager handlingManager,
			IStreamStateRepo streamStateRepo,
			SubscriberOptions options)
		{
			_logger = logger;
			_streamClientFactory = streamClientFactory;
			_subscriptionListener = subscriptionListener;
			_resolutionManager = resolutionManager;
			_sortingManager = sortingManager;
			_handlingManager = handlingManager;
			_streamStateRepo = streamStateRepo;
			_options = options;
		}

		public async Task SubscribeAsync(CancellationToken cancellationToken)
		{
			if (_isSubscribing) throw new InvalidOperationException("Already subscribing.");

			_isSubscribing = true;

			var tasks = new List<Task>();

			// One subscription listener for each region.
			foreach (var subStreamId in _options.SubscriptionStreamIds)
			{
				tasks.Add(_subscriptionListener.ListenAsync(subStreamId.RegionId, subStreamId.StreamId, cancellationToken));
			}

			tasks.Add(_resolutionManager.ManageAsync(cancellationToken));
			tasks.Add(_sortingManager.ManageAsync(cancellationToken));
			tasks.Add(_handlingManager.ManageAsync(cancellationToken));

			tasks.Add(cancellationToken.WaitHandle.AsTask());

			await Task.WhenAny(tasks);

			_isSubscribing = false;
		}

		public async Task ResetStreamStatesAsync()
		{
			if (_isSubscribing)
				throw new InvalidOperationException("Can't reset stream states while subscribing.");

			try
			{
				await _streamStateRepo.ResetStreamStatesAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while resetting stream states.");
				throw;
			}
		}

		public async Task ClearStreamStateErrorsAsync()
		{
			if (_isSubscribing)
				throw new InvalidOperationException("Can't clear stream state errors while subscribing.");

			try
			{
				await _streamStateRepo.ClearStreamStateErrorsAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while clearing stream state errors.");
				throw;
			}
		}

		public async Task<IDictionary<string, long?>> GetEndsOfSubscriptionAsync()
		{
			try
			{
				var ends = new Dictionary<string, long?>();

				foreach (var subStreamId in _options.SubscriptionStreamIds)
				{
					using (var streamClient = _streamClientFactory.Create(subStreamId.RegionId))
					{
						var position = await streamClient.GetLastPositionInStreamAsync(subStreamId.StreamId);
						ends.Add(subStreamId.StreamId, position);
					}
				}

				return ends;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while clearing stream state errors.");
				throw;
			}
		}
	}
}
