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
		private readonly Func<IBusinessEventResolver, ISerializableAggregateRootStateObjectRepo, TState> _stateConstructor;

		public SerializableAggregateRootStateFactory(
			IBusinessEventResolver resolver, ISerializableAggregateRootStateObjectRepo repo,
			Func<IBusinessEventResolver, ISerializableAggregateRootStateObjectRepo, TState> stateConstructor)
		{
			_resolver = resolver;
			_repo = repo;
			_stateConstructor = stateConstructor;
		}

		public async Task<TState> CreateAndLoadToCheckpointAsync(string regionId, string context, string aggregateRootName, string aggregateRootId, CancellationToken cancellationToken)
		{
			// This is kind of a frankenstein factory method where the actual construction of the object is deferred to
			// the implementing class. We do this because we want a structured way to instantiate a serializable state, but
			// we don't know exactly what the implementing states' constructors will need as parameters.
			var state = _stateConstructor(_resolver, _repo);
			await state.InitializeAsync(regionId, context, aggregateRootName, aggregateRootId, cancellationToken);
			return state;
		}
	}
}
