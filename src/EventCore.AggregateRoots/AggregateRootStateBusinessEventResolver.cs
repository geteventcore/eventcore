using EventCore.EventSourcing;
using EventCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventCore.AggregateRoots
{
	public class AggregateRootStateBusinessEventResolver<TState> : BusinessEventResolver
		where TState : IAggregateRootState
	{
		// Business event resolver that automatically determines resolvable events based on IApplyBusinessEvent generic parameters.
		public AggregateRootStateBusinessEventResolver(IStandardLogger logger) : base(logger, GetAppliedBusinessEventTypes())
		{
		}

		public static ISet<Type> GetAppliedBusinessEventTypes()
		{
			return new HashSet<Type>(typeof(TState).GetInterfaces().Where(x => x == typeof(IApplyBusinessEvent<>)).Select(x => x.GetGenericArguments()[0]).Distinct());
		}
	}
}
