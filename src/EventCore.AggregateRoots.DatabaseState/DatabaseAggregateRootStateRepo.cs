using EventCore.AggregateRoots;
using EventCore.EventSourcing;
using EventCore.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots.DatabaseState
{
	public abstract class DatabaseAggregateRootStateRepo<TState> : IAggregateRootStateRepo<TState>
		where TState : IDatabaseAggregateRootState
	{
		private readonly IStreamClientFactory _streamClientFactory;
		private readonly Func<string, TState> _stateConstructor;
		private readonly int _saveBatchSize;

		public DatabaseAggregateRootStateRepo(IStreamClientFactory streamClientFactory, Func<string, TState> stateConstructor, int saveBatchSize)
		{
			_streamClientFactory = streamClientFactory;
			_stateConstructor = stateConstructor;
			_saveBatchSize = saveBatchSize;
		}

		public async Task<TState> LoadAsync(string regionId, string streamId, CancellationToken cancellationToken)
		{
			var state = _stateConstructor(regionId);
			await HydrateFromCheckpointAsync(state, regionId, streamId, cancellationToken);
			return state;
		}

		protected virtual async Task HydrateFromCheckpointAsync(TState state, string regionId, string streamId, CancellationToken cancellationToken)
		{
			var streamClient = _streamClientFactory.Create(regionId);
			var fromPosition = state.StreamPositionCheckpoint.GetValueOrDefault(streamClient.FirstPositionInStream - 1) + 1;

			var callCount = 1;
			await streamClient.LoadStreamEventsAsync(
				streamId, fromPosition,
				async se =>
				{
					await state.ApplyStreamEventAsync(se, cancellationToken);
					if (callCount % _saveBatchSize == 0)
					{
						await state.SaveChangesAsync(cancellationToken);
					}
					callCount++;
				},
				cancellationToken
			);
		}
	}
}