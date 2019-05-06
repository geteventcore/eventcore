using EventCore.EventSourcing;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots.SerializableState
{
	public class SerializableAggregateRootStateFileRepo<TState> : IAggregateRootStateRepo<TState>
		where TState : ISerializableAggregateRootState
	{
		private readonly IStreamClientFactory _streamClientFactory;
		private readonly Func<TState> _stateConstructor;
		private readonly string _basePath;

		public SerializableAggregateRootStateFileRepo(IStreamClientFactory streamClientFactory, Func<TState> stateConstructor, string basePath)
		{
			_streamClientFactory = streamClientFactory;
			_stateConstructor = stateConstructor;

			_basePath = Path.GetFullPath(basePath);

			// Initialize state storage directory.
			if (!Directory.Exists(_basePath))
			{
				Directory.CreateDirectory(_basePath);
			}
		}

		public virtual async Task<TState> LoadAsync(string regionId, string streamId, CancellationToken cancellationToken)
		{
			var state = _stateConstructor();
			var stateFilePath = BuildStateFilePath(regionId, streamId);

			// Load cached state if exists.
			if (File.Exists(stateFilePath))
			{
				var data = File.ReadAllBytes(stateFilePath); // Async read not avialable in this .NET Standard version.
				await state.DeserializeInternalStateAsync(data);
			}

			// Catch up on latest events.
			await HydrateFromCheckpointAsync(state, regionId, streamId, cancellationToken);

			return state;
		}

		protected virtual async Task HydrateFromCheckpointAsync(TState state, string regionId, string streamId, CancellationToken cancellationToken)
		{
			var streamClient = _streamClientFactory.Create(regionId);
			var fromPosition = state.StreamPositionCheckpoint.GetValueOrDefault(streamClient.FirstPositionInStream - 1) + 1;
			await streamClient.LoadStreamEventsAsync(streamId, fromPosition, se => state.ApplyStreamEventAsync(se, cancellationToken), cancellationToken);
		}

		protected virtual string BuildStateFilePath(string regionId, string streamId)
		{
			return Path.Combine(_basePath, regionId, streamId);
		}
	}
}
