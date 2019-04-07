using System.IO;
using System.Threading.Tasks;

namespace EventCore.StatefulSubscriber
{
	public class StreamStateRepo : IStreamStateRepo
	{
		private readonly string _basePath;

		public StreamStateRepo(string basePath)
		{
			_basePath = Path.GetFullPath(basePath);
		}

		// NOTE: Stream ids are lower-cased to be case INsensitive, expecting ascii characters only
		// since stream ids are controlled internally by development team, not subject to external input.
		private string BuildStreamStateFilePath(string streamId) => Path.Combine(_basePath, streamId.ToLower());

		public Task<StreamState> LoadStreamStateAsync(string streamId)
		{
			var stateFilePath = BuildStreamStateFilePath(streamId);

			if (File.Exists(stateFilePath))
			{
				bool lastProcessedPositionHasValue;
				long lastProcessedPositionValue;
				long? lastProcessedPosition;
				bool hasError;

				// FileShare.ReadWrite will allow other code to read but not write while we're reading the file.
				using (BinaryReader reader = new BinaryReader(File.Open(stateFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
				{
					lastProcessedPositionHasValue = reader.ReadBoolean();
					lastProcessedPositionValue = reader.ReadInt64();
					hasError = reader.ReadBoolean();
				}

				if (lastProcessedPositionHasValue) lastProcessedPosition = lastProcessedPositionValue;
				else lastProcessedPosition = null;

				return Task.FromResult<StreamState>(new StreamState(lastProcessedPosition, hasError));
			}
			else
			{
				return Task.FromResult<StreamState>(null);
			}
		}

		public Task SaveStreamStateAsync(string streamId, long? lastProcessedPosition, bool hasError)
		{
			var stateFilePath = BuildStreamStateFilePath(streamId);

			// FileMode.Create will overwrite the file if exists - this is what we want.
			// FileShare.None will prevent other code from access while we're writing.
			using (BinaryWriter writer = new BinaryWriter(File.Open(stateFilePath, FileMode.Create, FileAccess.Write, FileShare.None)))
			{
				writer.Write(lastProcessedPosition.HasValue);
				writer.Write(lastProcessedPosition.GetValueOrDefault());
				writer.Write(hasError);
			}

			return Task.CompletedTask;
		}
	}
}
