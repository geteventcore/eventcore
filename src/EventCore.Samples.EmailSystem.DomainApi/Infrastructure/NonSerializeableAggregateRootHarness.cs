﻿using EventCore.AggregateRoots;
using EventCore.Samples.EmailSystem.Domain;
using System;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.DomainApi.Infrastructure
{
	public class NonSerializeableAggregateRootHarness<TRoot, TState> : IAggregateRootHarness<TRoot, TState>
		where TRoot : AggregateRoot<TState>
		where TState : IAggregateRootState
	{
		public NonSerializeableAggregateRootHarness()
		{
		}

		public Task<IHandledCommandResult> HandleCommandAsync<TCommand>(TCommand c) where TCommand : DomainCommand
		{
			throw new NotImplementedException();
		}
	}
}
