using EventCore.Samples.GYEventStore.StreamClient;
using EventCore.Utilities;
using EventStore.ClientAPI;
using Microsoft.Extensions.Configuration;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Cli.Actions
{
	public class ListenAction : IAction
	{
		private readonly Options.ListenOptions _options;
		private readonly IStandardLogger _logger;
		private readonly IConfiguration _config;

		public ListenAction(Options.ListenOptions options, IStandardLogger logger, IConfiguration config)
		{
			_options = options;
			_logger = logger;
			_config = config;
		}

		public async Task RunAsync()
		{
			Console.WriteLine("Listening for all non-system events... ");
			Console.WriteLine();

			using (var eventStoreConnection = EventStoreConnection.Create(_config.GetConnectionString("EventStoreRegionX")))
			{
				eventStoreConnection.Closed += new EventHandler<ClientClosedEventArgs>(delegate (Object o, ClientClosedEventArgs a)
				{
					Console.WriteLine($"Event Store connection closed. Reconnecting after delay.");
					Thread.Sleep(1);
					a.Connection.ConnectAsync().Wait();
				});

				await eventStoreConnection.ConnectAsync();

				using (var streamClient = new EventStoreStreamClient(_logger, eventStoreConnection, new EventStoreStreamClientOptions(100)))
				{
					var streamId = "$allNonSystemEvents";
					var lastGlobalPosition = await streamClient.GetLastPositionInStreamAsync(streamId);
					await streamClient.SubscribeToStreamAsync(
						streamId,
						lastGlobalPosition.GetValueOrDefault(streamClient.FirstPositionInStream - 1) + 1,
						(se) =>
						{
							if (se.IsLink)
							{
								Console.WriteLine($"Event: {se.EventType} ({se.Link.Position}) in {se.Link.StreamId}");
							}
							else
							{
								Console.WriteLine($"Event: {se.EventType} ({se.Position}) in {se.StreamId}");
							}

							if (_options.Verbose)
							{
								Console.WriteLine(Encoding.Unicode.GetString(se.Data));
							}
							return Task.CompletedTask;
						},
						CancellationToken.None);
				}
			}
		}
	}
}
