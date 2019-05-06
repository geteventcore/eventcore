using EventCore.EventSourcing;
using EventCore.Utilities;

namespace EventCore.AggregateRoots
{
	public class AggregateRootDependencies<TState> where TState : IAggregateRootState
	{
		public readonly IStandardLogger Logger;
		public readonly IAggregateRootStateRepo<TState> StateRepo;
		public readonly IStreamIdBuilder StreamIdBuilder;
		public readonly IStreamClientFactory StreamClientFactory;
		public readonly IBusinessEventResolver EventResolver;

		public AggregateRootDependencies(
			IStandardLogger logger,
			IAggregateRootStateRepo<TState> stateRepo, IStreamIdBuilder streamIdBuilder,
			IStreamClientFactory streamClientFactory, IBusinessEventResolver eventResolver)
		{
			Logger = logger;
			StateRepo = stateRepo;
			StreamIdBuilder = streamIdBuilder;
			StreamClientFactory = streamClientFactory;
			EventResolver = eventResolver;
		}
	}
}
