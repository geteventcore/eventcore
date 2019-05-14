using EventCore.Samples.GYEventStore.StreamClient;
using EventCore.Utilities;
using EventStore.ClientAPI;
using System;
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
			Console.WriteLine();

			using (var eventStoreConnection = EventStoreConnection.Create(_eventStoreUri, ""))
			{
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
								Console.WriteLine($"Event: {se.EventType} ({se.Link.Position}) in {se.Link.StreamId})");
							}
							else
							{
								Console.WriteLine($"Event: {se.EventType} ({se.Position}) in {se.StreamId})");
							}
							return Task.CompletedTask;
						},
						CancellationToken.None);
				}
			}
		}
	}
}
