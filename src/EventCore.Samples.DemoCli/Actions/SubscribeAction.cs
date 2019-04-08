using EventCore.EventSourcing;
using EventCore.EventSourcing.EventStore;
using EventCore.Samples.DemoCli.BasicBusinessEvents;
using EventCore.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.DemoCli.Actions
{
	public class SubscribeAction : IAction
	{
		private readonly Options.SubscribeOptions _options;

		public SubscribeAction(Options.SubscribeOptions options)
		{
			_options = options;
		}

		public async Task RunAsync()
		{
			var streamId = Constants.STREAM_ID_ALL;

			Console.WriteLine($"Subscribing to events on {streamId} stream.");
			Console.WriteLine();

			var serializer = new JsonBusinessEventSerializer();
			var options = new EventStoreStreamClientOptions(100); // Read batch size not used here.
			var streamClient = new EventStoreStreamClient(NullStandardLogger.Instance, Helpers.EventStoreConnectionFactory, options);

			Console.WriteLine("Current end of stream: " + (await streamClient.FindLastPositionInStreamAsync(Constants.EVENTSTORE_DEFAULT_REGION, streamId)).GetValueOrDefault(streamClient.FirstPositionInStream - 1));
			Console.WriteLine();

			var cancelSource = new CancellationTokenSource();

			var _ = streamClient.SubscribeToStreamAsync(
				Constants.EVENTSTORE_DEFAULT_REGION, streamId, _options.FromPosition,
				(se, ct) =>
				{
					var linkMsg = se.IsLink ? $" [Linked to {se.Link.StreamId} ({se.Link.Position}).]" : "";
					var type = MapEventType(se.EventType);
					var e = (BaseBusinessEvent)serializer.DeserializeEvent(type, se.Data);

					Console.WriteLine($"Received event type {type.Name} from {se.StreamId} ({se.Position})." + linkMsg);
					Console.WriteLine($"Message: {e.Message}");
					Console.WriteLine();
					
					
					return Task.CompletedTask;
				},
				cancelSource.Token);

			Console.WriteLine("Press ENTER to stop.");
			Console.WriteLine();
			Console.ReadLine();
		}

		private Type MapEventType(string eventType)
		{
			eventType = eventType.ToUpper();

			if (eventType == typeof(BusinessEventT01).Name.ToUpper()) return typeof(BusinessEventT01);
			if (eventType == typeof(BusinessEventT02).Name.ToUpper()) return typeof(BusinessEventT02);
			if (eventType == typeof(BusinessEventT03).Name.ToUpper()) return typeof(BusinessEventT03);
			if (eventType == typeof(BusinessEventT04).Name.ToUpper()) return typeof(BusinessEventT04);
			if (eventType == typeof(BusinessEventT05).Name.ToUpper()) return typeof(BusinessEventT05);
			if (eventType == typeof(BusinessEventT06).Name.ToUpper()) return typeof(BusinessEventT01);
			if (eventType == typeof(BusinessEventT07).Name.ToUpper()) return typeof(BusinessEventT07);
			if (eventType == typeof(BusinessEventT08).Name.ToUpper()) return typeof(BusinessEventT08);
			if (eventType == typeof(BusinessEventT09).Name.ToUpper()) return typeof(BusinessEventT09);
			if (eventType == typeof(BusinessEventT10).Name.ToUpper()) return typeof(BusinessEventT10);

			return typeof(object);
		}
	}
}
