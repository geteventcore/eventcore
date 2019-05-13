using EventCore.Samples.SimpleEventStore.Client;
using EventCore.Samples.SimpleEventStore.StreamDb;
using EventCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.SimpleEventStore.Cli.Actions
{
	public class SubscribeAction : IAction
	{
		private readonly Options.SubscribeOptions _options;
		private readonly IStandardLogger _logger;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly string _notificationsHubUrl;

		public SubscribeAction(Options.SubscribeOptions options, IStandardLogger logger, IServiceScopeFactory scopeFactory, string notificationsHubUrl)
		{
			_options = options;
			_logger = logger;
			_scopeFactory = scopeFactory;
			_notificationsHubUrl = notificationsHubUrl;
		}

		public async Task RunAsync()
		{
			Console.WriteLine("Subscribing to all events... (break to exit)");

			using (var scope = _scopeFactory.CreateScope())
			{
				var db = scope.ServiceProvider.GetRequiredService<StreamDbContext>();

				using (var client = new StreamClient(_logger, db, _notificationsHubUrl))
				{
					var lastGlobalPosition = await client.GetLastPositionInStreamAsync("$");
					await client.SubscribeToStreamAsync(
						"$",
						lastGlobalPosition.GetValueOrDefault(client.FirstPositionInStream - 1) + 1,
						(se) =>
						{
							_logger.LogInformation($"{se.EventType} ({se.Link.Position} in {se.Link.StreamId})");
							return Task.CompletedTask;
						},
						CancellationToken.None);
				}
			}
		}
	}
}
