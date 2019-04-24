using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace EventCore.AggregateRoots.SerializableState.Tests
{
	public class DiskSerializableAggregateRootStateObjectRepoTests
	{
		private class TestInternalState { }

		[Fact]

		public void build_state_file_path()
		{
			var basePath = Directory.GetCurrentDirectory();
			var repo = new FileSerializableAggregateRootStateObjectRepo(basePath);
			var regionId = "x";
			var context = "ctx";
			var aggregateRootName = "ar";
			var aggregateRootId = "1";

			var expectedPath = Path.Combine(basePath, string.Join("-", regionId, context, aggregateRootName, aggregateRootId));
			var actualPath = FileSerializableAggregateRootStateObjectRepo.BuildStateFilePath(basePath, regionId, context, aggregateRootName, aggregateRootId);

			Assert.Equal(expectedPath, actualPath);
		}

		[Fact]
		public async Task save_and_load_state()
		{
			var basePath = Directory.GetCurrentDirectory();
			var repo = new FileSerializableAggregateRootStateObjectRepo(basePath);
			var regionId = "x";
			var context = "ctx";
			var aggregateRootName = "ar";
			var aggregateRootId = "1";
			var streamPositionCheckpoint = 1;
			var causalId = "abc";
			var internalState = new TestInternalState();
			var stateObj = new SerializableAggregateRootStateObject<TestInternalState>(streamPositionCheckpoint, new List<string>() { causalId }, internalState);

			await repo.SaveAsync(regionId, context, aggregateRootName, aggregateRootId, stateObj);
			var loadedState = await repo.LoadAsync<TestInternalState>(regionId, context, aggregateRootName, aggregateRootId);

			Assert.Equal(streamPositionCheckpoint, loadedState.StreamPositionCheckpoint);
			Assert.Contains(causalId, loadedState.CausalIdHistory);
			Assert.NotNull(loadedState.InternalState);
		}
	}
}
