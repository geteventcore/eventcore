using EventCore.EventSourcing.EventStore;
using EventCore.Utilities;
using System;
using System.Threading.Tasks;

namespace EventCore.Samples.DemoCli.Actions
{
	public class CommitAction : IAction
	{
		public CommitAction(Options.CommitOptions options)
		{
		}

		public Task RunAsync()
		{
			Console.WriteLine("Running demo...");
			return Task.CompletedTask;
		}

		private Task CommitEventGroupToAggregateStreamAsync(char aggId)
		{
			var options = new EventStoreStreamClientOptions(100); // Read batch size not used here.
			var conn = EventStore.ClientAPI.EventStoreConnection.Create(Constants.EVENTSTORE_URI);
			var streamClient = new EventStoreStreamClient(NullGenericLogger.Instance, () => conn, options);

			return Task.CompletedTask;
		}
	}
}
