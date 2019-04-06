using EventCore.EventSourcing;
using EventCore.EventSourcing.EventStore;
using EventCore.Samples.DemoCli.BasicBusinessEvents;
using EventCore.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventCore.Samples.DemoCli.Actions
{
	public class CommitAction : IAction
	{
		private readonly Options.CommitOptions _options;

		public CommitAction(Options.CommitOptions options)
		{
			_options = options;
		}

		public async Task RunAsync()
		{
			for (var i = 1; i <= _options.EventGroupsPerStream; i++)
			{
				long? startPosition = (i == 1) ? (long?)null : (i - 1) * _options.EventGroupsPerStream;

				await CommitEventsForAggregateType('A', startPosition);
				await CommitEventsForAggregateType('B', startPosition);
				await CommitEventsForAggregateType('C', startPosition);
				await CommitEventsForAggregateType('D', startPosition);
				await CommitEventsForAggregateType('E', startPosition);
				await CommitEventsForAggregateType('F', startPosition);
			}
		}

		private async Task CommitEventsForAggregateType(char type, long? startPosition)
		{
			for (var i = 1; i <= _options.StreamsPerAgg; i++)
			{
				await CommitEventGroupToAggregateStreamAsync($"Agg{type}-{i.ToString("00")}", startPosition);
			}
		}

		private async Task CommitEventGroupToAggregateStreamAsync(string streamId, long? startPosition)
		{
			var serializer = new JsonBusinessEventSerializer();
			var options = new EventStoreStreamClientOptions(100); // Read batch size not used here.
			var streamClient = new EventStoreStreamClient(NullGenericLogger.Instance, Helpers.EventStoreConnectionFactory, options);
			long position;


			position = startPosition.GetValueOrDefault(streamClient.FirstPositionInStream);
			var e1 = new BusinessEventT01($"{typeof(BusinessEventT01).Name} committing to {streamId} at {position}.");
			var e2 = new BusinessEventT02($"{typeof(BusinessEventT02).Name} committing to {streamId} at {position++}.");
			var e3 = new BusinessEventT03($"{typeof(BusinessEventT03).Name} committing to {streamId} at {position++}.");
			var e4 = new BusinessEventT04($"{typeof(BusinessEventT04).Name} committing to {streamId} at {position++}.");
			var e5 = new BusinessEventT05($"{typeof(BusinessEventT05).Name} committing to {streamId} at {position++}.");
			var e6 = new BusinessEventT06($"{typeof(BusinessEventT06).Name} committing to {streamId} at {position++}.");
			var e7 = new BusinessEventT07($"{typeof(BusinessEventT07).Name} committing to {streamId} at {position++}.");
			var e8 = new BusinessEventT08($"{typeof(BusinessEventT08).Name} committing to {streamId} at {position++}.");
			var e9 = new BusinessEventT09($"{typeof(BusinessEventT09).Name} committing to {streamId} at {position++}.");
			var e10 = new BusinessEventT10($"{typeof(BusinessEventT10).Name} committing to {streamId} at {position++}.");

			var events = new List<BusinessEvent>() { e1, e2, e3, e4, e5, e6, e7, e8, e9, e10 };
			var commitEvents = new List<CommitEvent>();

			position = startPosition.GetValueOrDefault(streamClient.FirstPositionInStream);
			foreach (var e in events)
			{
				commitEvents.Add(new CommitEvent(e.GetType().Name, serializer.SerializeEvent(e)));
				Console.WriteLine($"Committing {e.GetType().Name} to {streamId} at position {position++}.");
			}

			long? expectedLastPosition = null;
			if (startPosition.GetValueOrDefault(streamClient.FirstPositionInStream) > streamClient.FirstPositionInStream)
			{
				expectedLastPosition = startPosition.Value - 1;
			}

			var result = await streamClient.CommitEventsToStreamAsync(Constants.EVENTSTORE_DEFAULT_REGION, streamId, expectedLastPosition, commitEvents);

			switch(result)
			{
				case CommitResult.Success: break;
				case CommitResult.ConcurrencyConflict: throw new Exception("Concurrency conflict.");
			}

			Console.WriteLine("Events committed.");
		}
	}
}
