using EventCore.Samples.SimpleEventStore.EventStoreDb;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.SimpleEventStore.NotificationsApi.HostedServices
{
	public class PollingBackgroundService : BackgroundService
	{
		private const int POLLING_INTERVAL_MS = 10000;
		private readonly ILogger _logger;
		private readonly NotificationsManager _manager;
		private readonly IServiceScopeFactory _scopeFactory;

		public PollingBackgroundService(ILogger<PollingBackgroundService> logger, NotificationsManager manager, IServiceScopeFactory scopeFactory)
		{
			_logger = logger;
			_manager = manager;
			_scopeFactory = scopeFactory;
		}

		protected async override Task ExecuteAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Polling background service is starting.");

			// Polls the event store database periodically to ensure subscribers always
			// have the latest events. This is a crude but simple way to prevent
			// scenarios where a new event notification never reaches the server, i.e.
			// polling is a back up plan in case the client doesn't properly notify the server.
			// Although polling has a fixed delay, it ensures event notifications are eventually
			// sent to subscribers.

			using (var scope = _scopeFactory.CreateScope())
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					var db = scope.ServiceProvider.GetRequiredService<EventStoreDbContext>();

					var latestGlobalPosition = db.GetMaxGlobalPosition();

					if (latestGlobalPosition.HasValue)
					{
						await _manager.TryUpdateGlobalPositionAsync(latestGlobalPosition.Value);
					}

					await Task.Delay(POLLING_INTERVAL_MS, cancellationToken);
				}
			}

			_logger.LogInformation("Polling background service is stopping.");
		}
	}
}
