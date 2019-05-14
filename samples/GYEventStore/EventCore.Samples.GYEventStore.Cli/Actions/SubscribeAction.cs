using EventCore.Samples.GYEventStore.StreamClient;
using EventCore.Utilities;
using EventStore.ClientAPI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.GYEventStore.Cli.Actions
{
	public class SubscribeAction : IAction
	{
		private readonly Options.SubscribeOptions _options;
		private readonly IStandardLogger _logger;
		private readonly string _eventStoreUri;

		public SubscribeAction(Options.SubscribeOptions options, IStandardLogger logger, string eventStoreUri)
		{
			_options = options;
			_logger = logger;
			_eventStoreUri = eventStoreUri;
		}

		public async Task RunAsync()
		{
			Console.WriteLine("Subscribing to all events... (break to exit)");

			using (var eventStoreConnection = EventStoreConnection.Create(_eventStoreUri, ""))
			{
				await eventStoreConnection.ConnectAsync();

				using (var streamClient = new EventStoreStreamClient(_logger, eventStoreConnection, new EventStoreStreamClientOptions(100)))
				{
					var lastGlobalPosition = await streamClient.GetLastPositionInStreamAsync("$all");
					await streamClient.SubscribeToStreamAsync(
						"$all",
						lastGlobalPosition.GetValueOrDefault(streamClient.FirstPositionInStream - 1) + 1,
						(se) =>
						{
							if (se.IsLink)
							{
								_logger.LogInformation($"{se.EventType} ({se.Link.Position} in {se.Link.StreamId})");
							}
							else
							{
								_logger.LogInformation($"{se.EventType} ({se.Position} in {se.StreamId})");
							}
							return Task.CompletedTask;
						},
						CancellationToken.None);
				}
			}
		}
	}
}
