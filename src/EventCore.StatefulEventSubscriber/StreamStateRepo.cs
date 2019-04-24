using EventCore.Utilities;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.StatefulEventSubscriber
{
	public class StreamStateRepo : IStreamStateRepo
	{
		public const string STREAM_PATH_PREFIX = "streams";

		private readonly IStandardLogger _logger;
		private readonly string _basePath;
		private readonly bool _simulateErrorInRetryLoops = false;

		public string BasePath { get => _basePath; }

		public StreamStateRepo(IStandardLogger logger, string basePath)
		{
			_logger = logger;
			_basePath = Path.GetFullPath(basePath);

			var dirPath = BuildStreamStateDirectoryPath();
			if (!Directory.Exists(dirPath))
			{
				Directory.CreateDirectory(dirPath);
			}
		}

		// For testing.
		public StreamStateRepo(IStandardLogger logger, string basePath, bool simulateErrorsInRetryLoops) : this(logger, basePath)
		{
			_simulateErrorInRetryLoops = simulateErrorsInRetryLoops;
		}

		// NOTE: Stream ids are lower-cased to be case INsensitive, expecting ascii characters only
		// since stream ids are controlled internally by development team, not subject to external input.
		// Also, stream state files are prefixed by a subdirectory to ensure that when we reset state
		// we don't just delete the whole base path directory, but instead delete well-defined paths. This
		// to guard against accidentally deleting an important directory if the setting provided is a root path
		// or some other important directory.
		public string BuildStreamStateFilePath(string streamId) => Path.Combine(_basePath, STREAM_PATH_PREFIX, streamId.ToLower());
		public string BuildStreamStateDirectoryPath() => Path.Combine(_basePath, STREAM_PATH_PREFIX + Path.DirectorySeparatorChar);

		public async Task<StreamState> LoadStreamStateAsync(string streamId)
		{
			var retry = false;
			var tryCount = 1;

			do
			{
				try
				{
					if (_simulateErrorInRetryLoops) throw new Exception("Simulated.");

					var stateFilePath = BuildStreamStateFilePath(streamId);

					return TryLoadStreamState(stateFilePath);
				}
				catch (Exception ex)
				{
					var delayMs = 1000;
					if (tryCount == 2) delayMs = 5000;
					if (tryCount == 3) delayMs = 30000;

					if (tryCount == 4)
					{
						retry = false;
						_logger.LogError(ex, "Exception while saving stream state. Giving up.");
						throw;
					}
					else
					{
						retry = true;
						_logger.LogError(ex, $"Exception while saving stream state. Waiting {delayMs}ms and trying again.");
						await Task.Delay(1000);
					}

					tryCount++;
				}
			} while (retry);

			return null;
		}

		private StreamState TryLoadStreamState(string stateFilePath)
		{
			if (File.Exists(stateFilePath))
			{
				long lastAttemptedPosition;
				bool hasError;

				// FileShare.ReadWrite will allow other code to read but not write while we're reading the file.
				using (BinaryReader reader = new BinaryReader(File.Open(stateFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
				{
					lastAttemptedPosition = reader.ReadInt64();
					hasError = reader.ReadBoolean();
				}

				return new StreamState(lastAttemptedPosition, hasError);
			}
			return null;
		}

		public async Task SaveStreamStateAsync(string streamId, long lastAttemptedPosition, bool hasError)
		{
			var retry = false;
			var tryCount = 1;

			do
			{
				try
				{
					if (_simulateErrorInRetryLoops) throw new Exception("Simulated.");

					var stateFilePath = BuildStreamStateFilePath(streamId);

					// FileMode.Create will overwrite the file if exists - this is what we want.
					// FileShare.None will prevent other code from access while we're writing.
					using (BinaryWriter writer = new BinaryWriter(File.Open(stateFilePath, FileMode.Create, FileAccess.Write, FileShare.None)))
					{
						writer.Write(lastAttemptedPosition);
						writer.Write(hasError);
					}
				}
				catch (Exception ex)
				{
					var delayMs = 1000;
					if (tryCount == 2) delayMs = 5000;
					if (tryCount == 3) delayMs = 30000;

					if (tryCount == 4)
					{
						retry = false;
						_logger.LogError(ex, "Exception while saving stream state. Giving up.");
						throw;
					}
					else
					{
						retry = true;
						_logger.LogError(ex, $"Exception while saving stream state. Waiting {delayMs}ms and trying again.");
						await Task.Delay(1000);
					}

					tryCount++;
				}
			} while (retry);
		}

		public Task ResetStreamStatesAsync()
		{
			Directory.Delete(BuildStreamStateDirectoryPath(), true);
			return Task.CompletedTask;
		}

		public async Task ClearStreamStateErrorsAsync(CancellationToken cancellationToken)
		{
			foreach (var stateFilePath in Directory.EnumerateFiles(BuildStreamStateDirectoryPath()))
			{
				if(cancellationToken.IsCancellationRequested) return;
				
				var state = TryLoadStreamState(stateFilePath);
				if(state != null && state.HasError)
				{
					await SaveStreamStateAsync(Path.GetFileName(stateFilePath), state.LastAttemptedPosition, false);
				}
			}
		}
	}
}
