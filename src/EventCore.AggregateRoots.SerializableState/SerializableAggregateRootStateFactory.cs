using EventCore.EventSourcing;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots.SerializableState
{
	public class SerializableAggregateRootStateFactory<TState, TInternalState> : IAggregateRootStateFactory<TState>
		where TState : ISerializableAggregateRootState<TInternalState>
	{
		private readonly IBusinessEventResolver _resolver;
		private readonly ISerializableAggregateRootStateObjectRepo _repo;
		private readonly Func<IBusinessEventResolver, ISerializableAggregateRootStateObjectRepo, string, string, string, string, TState> _stateConstructor;

		public SerializableAggregateRootStateFactory(
			IBusinessEventResolver resolver, ISerializableAggregateRootStateObjectRepo repo,
			Func<IBusinessEventResolver, ISerializableAggregateRootStateObjectRepo, string, string, string, string, TState> stateConstructor)
		{
			_resolver = resolver;
			_repo = repo;
			_stateConstructor = stateConstructor;
		}

		public async Task<TState> CreateAndLoadToCheckpointAsync(string regionId, string context, string aggregateRootName, string aggregateRootId, CancellationToken cancellationToken)
		{
			var state = _stateConstructor(_resolver, _repo, regionId, context, aggregateRootName, aggregateRootId);
			await state.InitializeAsync(cancellationToken);
			return state;
		}
	}
}
