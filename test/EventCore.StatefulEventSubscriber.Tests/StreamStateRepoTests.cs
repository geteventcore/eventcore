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
		public void construct_and_create_directory()
		{
			var basePath = Directory.GetCurrentDirectory();
			var repo = new StreamStateRepo(NullStandardLogger.Instance, basePath);
			Assert.Equal(basePath, repo.BasePath); ;
			Assert.True(Directory.Exists(repo.BuildStreamStateDirectoryPath()));
		}

		[Fact]
		public void build_stream_state_file_path()
		{
			var basePath = Directory.GetCurrentDirectory();
			var repo = new StreamStateRepo(NullStandardLogger.Instance, basePath);
			var streamId = "s1";
			var expectedPath = Path.Combine(basePath, StreamStateRepo.STREAM_PATH_PREFIX, streamId.ToLower());

			var actualPath = repo.BuildStreamStateFilePath(streamId);

			Assert.Equal(expectedPath, actualPath);
		}

		[Fact]
		public void build_stream_state_directory_path()
		{
			var basePath = Directory.GetCurrentDirectory();
			var repo = new StreamStateRepo(NullStandardLogger.Instance, basePath);
			var expectedPath = Path.Combine(basePath, StreamStateRepo.STREAM_PATH_PREFIX + Path.DirectorySeparatorChar);

			var actualPath = repo.BuildStreamStateDirectoryPath();

			Assert.Equal(expectedPath, actualPath);
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

		[Fact]
		public async Task reset_all_state()
		{
			var basePath = Directory.GetCurrentDirectory();
			var streamId1 = Guid.NewGuid().ToString();
			var streamId2 = Guid.NewGuid().ToString();
			var position1 = 1;
			var position2 = 1;
			var repo = new StreamStateRepo(NullStandardLogger.Instance, basePath);

			await repo.SaveStreamStateAsync(streamId1, position1, false);
			await repo.SaveStreamStateAsync(streamId2, position2, false);

			Assert.NotNull(await repo.LoadStreamStateAsync(streamId1));
			Assert.NotNull(await repo.LoadStreamStateAsync(streamId2));

			await repo.ResetAllStateAsync();

			Assert.Null(await repo.LoadStreamStateAsync(streamId1));
			Assert.Null(await repo.LoadStreamStateAsync(streamId2));
		}

		[Fact]
		public async Task rethrow_when_save_stream_state_error()
		{
			var basePath = Directory.GetCurrentDirectory();
			var streamId = Guid.NewGuid().ToString();
			var position = 5;
			var hasError = true;
			var repo = new StreamStateRepo(NullStandardLogger.Instance, basePath);

			await repo.SaveStreamStateAsync(streamId, position, hasError); // Successful save.

			// Manually open and lock the file.
			var filePath = repo.BuildStreamStateFilePath(streamId);
			using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None)))
			{
				await Assert.ThrowsAnyAsync<Exception>(() => repo.SaveStreamStateAsync(streamId, position, hasError));
			}
		}

		[Fact]
		public async Task rethrow_when_load_stream_state_error()
		{
			var basePath = Directory.GetCurrentDirectory();
			var streamId = Guid.NewGuid().ToString();
			var position = 5;
			var hasError = true;
			var repo = new StreamStateRepo(NullStandardLogger.Instance, basePath);

			await repo.SaveStreamStateAsync(streamId, position, hasError); // Successful save.

			// Manually open and lock the file.
			var filePath = repo.BuildStreamStateFilePath(streamId);
			using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None)))
			{
				await Assert.ThrowsAnyAsync<Exception>(() => repo.LoadStreamStateAsync(streamId));
			}
		}

		[Fact]
		public async Task rethrow_when_reset_all_state_error()
		{
			var basePath = Directory.GetCurrentDirectory();
			var streamId = Guid.NewGuid().ToString();
			var position = 5;
			var hasError = true;
			var repo = new StreamStateRepo(NullStandardLogger.Instance, basePath);

			await repo.SaveStreamStateAsync(streamId, position, hasError); // Successful save.

			// Manually open and lock the file.
			var filePath = repo.BuildStreamStateFilePath(streamId);
			using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None)))
			{
				await Assert.ThrowsAnyAsync<Exception>(() => repo.ResetAllStateAsync());
			}
		}
	}
}
