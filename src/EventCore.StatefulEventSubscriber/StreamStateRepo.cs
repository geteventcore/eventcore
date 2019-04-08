using System;
using System.IO;
using System.Threading.Tasks;
using EventCore.Utilities;

namespace EventCore.StatefulEventSubscriber
{
	public class StreamStateRepo : IStreamStateRepo
	{
		private readonly IStandardLogger _logger;
		private readonly string _basePath;

		public string BasePath { get => _basePath; }

		public StreamStateRepo(IStandardLogger logger, string basePath)
		{
			_logger = logger;
			_basePath = Path.GetFullPath(basePath);
		}

		// NOTE: Stream ids are lower-cased to be case INsensitive, expecting ascii characters only
		// since stream ids are controlled internally by development team, not subject to external input.
		private string BuildStreamStateFilePath(string streamId) => Path.Combine(_basePath, streamId.ToLower());

		public async Task<StreamState> LoadStreamStateAsync(string streamId)
		{
			var retry = true;
			var tryCount = 1;

			while (retry)
			{
				try
				{
					var stateFilePath = BuildStreamStateFilePath(streamId);

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
						_logger.LogError(ex, $"Exception while saving stream state. Waiting {delayMs}ms and trying again.");
						await Task.Delay(1000);
						tryCount++;
					}
				}
			}
			return null;
		}

		public async Task SaveStreamStateAsync(string streamId, long lastAttemptedPosition, bool hasError)
		{
			var retry = true;
			var tryCount = 1;

			while (retry)
			{
				try
				{
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
						_logger.LogError(ex, $"Exception while saving stream state. Waiting {delayMs}ms and trying again.");
						await Task.Delay(1000);
						tryCount++;
					}
				}
			}
		}
	}
}
