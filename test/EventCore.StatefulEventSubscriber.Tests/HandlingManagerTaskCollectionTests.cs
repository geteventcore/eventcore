using EventCore.EventSourcing;
using EventCore.Utilities;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventCore.StatefulEventSubscriber.Tests
{
	public class HandlingManagerTaskCollectionTests
	{
		[Fact]
		public void add_tasks()
		{
			var cts = new CancellationTokenSource();
			var parallelKey1 = "pk1";
			var parallelKey2 = "pk2";
			var task1 = Task.Run(() => cts.Token.WaitHandle.AsTask().Wait());
			var task2 = Task.Run(() => cts.Token.WaitHandle.AsTask().Wait());
			var collection = new HandlingManagerTaskCollection();

			collection.Add(parallelKey1, task1);
			collection.Add(parallelKey2, task2);

			Assert.Equal(2, collection.Keys.Count);

			cts.Cancel();
		}

		[Fact]
		public async Task purge_finished_tasks()
		{
			var cts1 = new CancellationTokenSource();
			var cts2 = new CancellationTokenSource();
			var parallelKey1 = "pk1";
			var parallelKey2 = "pk2";
			var task1 = Task.Run(() => cts1.Token.WaitHandle.AsTask().Wait());
			var task2 = Task.Run(() => cts2.Token.WaitHandle.AsTask().Wait());
			var collection = new HandlingManagerTaskCollection();

			collection.Add(parallelKey1, task1);
			collection.Add(parallelKey2, task2);

			cts1.Cancel();
			await task1;

			Assert.Equal(2, collection.Keys.Count);

			collection.PurgeFinishedTasks();

			Assert.Equal(1, collection.Keys.Count);

			cts2.Cancel();
			await task2;

			collection.PurgeFinishedTasks();
			Assert.Equal(0, collection.Keys.Count);
		}
	}
}
