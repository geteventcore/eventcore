using EventCore.EventSourcing;
using EventCore.Samples.Ecommerce.Events;
using EventCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EventCore.Samples.Ecommerce.Domain
{
	public class AllBusinessEventsResolver : BusinessEventResolver
	{
		// Business event resolver that automatically determines resolvable events from all domain events.
		public AllBusinessEventsResolver(IStandardLogger logger) : base(logger, GetAllBusinessEventTypes())
		{
		}

		public static ISet<Type> GetAllBusinessEventTypes()
		{
			var assembly = Assembly.GetAssembly(typeof(_Marker));
			var typeOfBusinessEvent = typeof(BusinessEvent);
			return new HashSet<Type>(assembly.GetTypes().Where(x => x.IsClass && x.IsSubclassOf(typeOfBusinessEvent)));
		}
	}
}