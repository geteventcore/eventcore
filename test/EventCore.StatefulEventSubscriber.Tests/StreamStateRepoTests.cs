using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using EventCore.Utilities;
using Xunit;

namespace EventCore.StatefulEventSubscriber.Tests
{
	public class StreamStateRepoTests
	{
		[Fact]
		public void construct()
		{
			var basePath = Directory.GetCurrentDirectory();
			var repo = new StreamStateRepo(NullStandardLogger.Instance, basePath);
			Assert.Equal(basePath, repo.BasePath); ;
		}

		[Fact]
		public async Task save_and_load_empty_state()
		{
			var basePath = Directory.GetCurrentDirectory();
			var streamId = Guid.NewGuid().ToString();
			var repo = new StreamStateRepo(NullStandardLogger.Instance, basePath);

			var state = await repo.LoadStreamStateAsync(streamId);

			Assert.Null(state);
		}

		[Fact]
		public async Task save_and_load_success_state()
		{
			var basePath = Directory.GetCurrentDirectory();
			var streamId = Guid.NewGuid().ToString();
			var position = 5;
			var hasError = false;
			var repo = new StreamStateRepo(NullStandardLogger.Instance, basePath);

			await repo.SaveStreamStateAsync(streamId, position, hasError);
			var state = await repo.LoadStreamStateAsync(streamId);

			Assert.Equal(position, state.LastAttemptedPosition);
			Assert.Equal(hasError, state.HasError);
		}

		[Fact]
		public async Task save_and_load_error_state()
		{
			var basePath = Directory.GetCurrentDirectory();
			var streamId = Guid.NewGuid().ToString();
			var position = 5;
			var hasError = true;
			var repo = new StreamStateRepo(NullStandardLogger.Instance, basePath);

			await repo.SaveStreamStateAsync(streamId, position, hasError);
			var state = await repo.LoadStreamStateAsync(streamId);

			Assert.Equal(position, state.LastAttemptedPosition);
			Assert.Equal(hasError, state.HasError);
		}

		[Fact]
		public async Task save_and_load_correct_stream()
		{
			var basePath = Directory.GetCurrentDirectory();
			var streamId1 = Guid.NewGuid().ToString();
			var streamId2 = Guid.NewGuid().ToString();
			var position1 = 1;
			var position2 = 1;
			var repo = new StreamStateRepo(NullStandardLogger.Instance, basePath);

			await repo.SaveStreamStateAsync(streamId1, position1, false);
			await repo.SaveStreamStateAsync(streamId2, position2, false);

			var state1 = await repo.LoadStreamStateAsync(streamId1);
			var state2 = await repo.LoadStreamStateAsync(streamId2);

			Assert.Equal(position1, state1.LastAttemptedPosition);
			Assert.Equal(position2, state2.LastAttemptedPosition);
		}
	}
}
