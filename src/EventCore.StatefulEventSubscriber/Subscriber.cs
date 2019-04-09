using EventCore.EventSourcing;
using EventCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulEventSubscriber
{
	public class Subscriber : ISubscriber
	{
		// General flow of execution...
		// 1. Stream events received from stream client listening on multiple regions.
		// 2. Stream events rejected if event type is not resolvable.
		// 3. Stream events enqueued for deserialization/resolution.
		// 4. Stream event deserialization attempted resulting in stateful subscriber event.
		// 5. Subscriber event enqueued for sorting into parallel handler executions.
		// 6. Handlers called in parallel by pulling events in order off of handling queues grouped by parallel key.
		//
		// Handling queues are identified by parallel keys, where each parallel queue has guaranteed
		// in-order events per stream. Multiple streams may be interleaved into one parallel queue,
		// and all events from a specific stream will be in order in any given parallel queue, but there is no
		// temporal guarantee across multiple parallel queues. Therefore, events in a stream may be handled out of
		// order between multiple parallel handlers, so it is up to the client implementation to sort events
		// into parallel keys with this consideration.

		private readonly IStandardLogger _logger;
		private readonly ISubscriptionListener _subListener;
		private readonly IResolutionManager _resolutionManager;
		private readonly ISortingManager _sortingManager;
		private readonly IHandlingManager _handlingManager;
		private readonly SubscriberOptions _options;

		private  bool _isSubscribing = false;

		public Subscriber(
			IStandardLogger logger, ISubscriptionListener subListener,
			IResolutionManager resolutionManager, ISortingManager sortingManager, IHandlingManager handlingManager,
			SubscriberOptions options)
		{
			_logger = logger;
			_subListener = subListener;
			_resolutionManager = resolutionManager;
			_sortingManager = sortingManager;
			_handlingManager = handlingManager;
			_options = options;
		}

		public async Task ResetStreamErrorStatesAsync()
		{
			if (_isSubscribing)
				throw new InvalidOperationException("Can't reset while subscribing.");

			await Task.Delay(10);
		}

		public async Task ResetAllStatesAsync()
		{
			if (_isSubscribing)
				throw new InvalidOperationException("Can't reset while subscribing.");

			await Task.Delay(10);
		}

		public async Task SubscribeAsync(IBusinessEventResolver resolver, Func<SubscriberEvent, string> sorter, Func<SubscriberEvent, CancellationToken, Task> handlerAsync, CancellationToken cancellationToken)
		{
			if (_isSubscribing) throw new InvalidOperationException("Already subscribing.");

			_isSubscribing = true;
			_logger.LogInformation("Stateful subscriber started.");

			var tasks = new List<Task>();

			// One subscription listener for each region.
			foreach (var regionId in _options.RegionIds.Distinct(StringComparer.OrdinalIgnoreCase))
			{
				tasks.Add(_subListener.ListenAsync(regionId, _options.StreamId, cancellationToken));
			}

			tasks.Add(_resolutionManager.ManageAsync(cancellationToken));
			tasks.Add(_sortingManager.ManageAsync(cancellationToken));
			tasks.Add(_handlingManager.ManageAsync(cancellationToken));

			tasks.Add(cancellationToken.WaitHandle.AsTask());

			await Task.WhenAny(tasks);

			_logger.LogInformation("Stateful subscriber stopped.");
			_isSubscribing = false;
		}
	}
}
