using EventCore.EventSourcing;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots.SerializableState
{
	public class SerializableAggregateRootStateFactory<TState, TInternalState> : IAggregateRootStateFactory<TState>
		where TState : SerializableAggregateRootState<TInternalState>
	{
		private readonly IBusinessEventResolver _resolver;
		private readonly ISerializableAggregateRootStateObjectRepo _repo;
		private readonly Func<string, string, string, string, IBusinessEventResolver, ISerializableAggregateRootStateObjectRepo, TState> _stateConstructor;

		public SerializableAggregateRootStateFactory(
			IBusinessEventResolver resolver, ISerializableAggregateRootStateObjectRepo repo,
			Func<string, string, string, string, IBusinessEventResolver, ISerializableAggregateRootStateObjectRepo, TState> stateConstructor)
		{
			_resolver = resolver;
			_repo = repo;
			_stateConstructor = stateConstructor;
		}

		public async Task<TState> CreateAndLoadToCheckpointAsync(string regionId, string context, string aggregateRootName, string aggregateRootId, CancellationToken cancellationToken)
		{
			var state = _stateConstructor(regionId, context, aggregateRootName, aggregateRootId, _resolver, _repo);
			await state.InitializeAsync(cancellationToken);
			return state;
		}
	}
}
