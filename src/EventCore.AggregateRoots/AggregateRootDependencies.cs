﻿using EventCore.EventSourcing;
using EventCore.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

		public AggregateRootDependencies(
			IStandardLogger logger,
			IAggregateRootStateFactory<TState> stateFactory, IStreamIdBuilder streamIdBuilder, IStreamClient streamClient,
			IBusinessEventResolver resolver, ICommandHandlerFactory<TState> handlerFactory)
		{
			Logger = logger;
			StateFactory = stateFactory;
			StreamIdBuilder = streamIdBuilder;
			StreamClient = streamClient;
			Resolver = resolver;
			HandlerFactory = handlerFactory;
		}
	}
}
