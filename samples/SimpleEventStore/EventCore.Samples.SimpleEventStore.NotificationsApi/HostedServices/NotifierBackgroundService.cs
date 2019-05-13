using EventCore.Utilities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.SimpleEventStore.NotificationsApi.HostedServices
{
	public class NotifierBackgroundService : BackgroundService
	{
		private const int NOTIFICATION_INTERVAL_MS = 1000;
		private readonly ILogger _logger;
		private readonly NotificationsManager _manager;
		private readonly IHubContext<NotificationsHub, INotificationsClient> _hub;

		public NotifierBackgroundService(ILogger<NotifierBackgroundService> logger, NotificationsManager manager, IHubContext<NotificationsHub, INotificationsClient> hub)
		{
			_logger = logger;
			_manager = manager;
			_hub = hub;
		}

		protected async override Task ExecuteAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Notifier background service is starting.");

			// Notifies connected SignalR clients as new events become available.
			// Notifications are sent:
			// A) Periodically to ensure subscribers don't miss a notification.
			// B) Immediately when global position is advanced, to minimize delay.

			// Note: No effort is made to keep track of which streams subscribers are interested
			// in. All subscribers receive all global position updates. This isn't very efficient
			// but since this is a simple event store demonstration it's not a priority.

			while (!cancellationToken.IsCancellationRequested)
			{
				if (_manager.GlobalPosition.HasValue)
				{
					await _hub.Clients.All.ReceiveClientNotification(_manager.GlobalPosition.Value);
				}
				await Task.WhenAny(new[] { _manager.GlobalPositionAdvancedSignal.WaitAsync(), Task.Delay(NOTIFICATION_INTERVAL_MS, cancellationToken) });
			}

			_logger.LogInformation("Notifier background service is stopping.");
		}
	}
}
