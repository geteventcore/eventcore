using EventCore.AggregateRoots;
using EventCore.EventSourcing;
using EventCore.Samples.Ecommerce.Domain.EmailBuilder.StateModels;
using System;
using System.Threading;

namespace EventCore.Samples.Ecommerce.Domain.EmailBuilder
{
	public class EmailBuilderStateFactory
	{
		private readonly IBusinessEventResolver _eventResolver;

		// Gives caller the option of separating state databases by region.
		private readonly Func<string, EmailBuilderDbContext> _dbFactory;

		public EmailBuilderStateFactory(IBusinessEventResolver resolver, Func<string, EmailBuilderDbContext> dbFactory)
		{
			_eventResolver = resolver;
			_dbFactory = dbFactory;
		}

		public EmailBuilderState Create(string regionId) => new EmailBuilderState(_eventResolver, _dbFactory(regionId));
	}
}