using EventCore.EventSourcing;
using EventCore.Utilities;

namespace EventCore.AggregateRoots
{
	public class AggregateRootDependencies<TState> where TState : IAggregateRootState
	{
		public readonly IStandardLogger Logger;
		public readonly IAggregateRootStateFactory<TState> StateFactory;
		public readonly IStreamIdBuilder StreamIdBuilder;
		public readonly IStreamClient StreamClient;
		public readonly IBusinessEventResolver Resolver;
		public readonly ICommandHandlerFactory<TState> HandlerFactory;
		public readonly ISerializedAggregateRootStateRepo SerializedAggregateRootStateRepo;

		public AggregateRootDependencies(
			IStandardLogger logger,
			IAggregateRootStateFactory<TState> stateFactory, IStreamIdBuilder streamIdBuilder, IStreamClient streamClient,
			IBusinessEventResolver resolver, ICommandHandlerFactory<TState> handlerFactory,
			ISerializedAggregateRootStateRepo serializedAggregateRootStateRepo)
		{
			Logger = logger;
			StateFactory = stateFactory;
			StreamIdBuilder = streamIdBuilder;
			StreamClient = streamClient;
			Resolver = resolver;
			HandlerFactory = handlerFactory;
			SerializedAggregateRootStateRepo = serializedAggregateRootStateRepo;
		}
	}
}
