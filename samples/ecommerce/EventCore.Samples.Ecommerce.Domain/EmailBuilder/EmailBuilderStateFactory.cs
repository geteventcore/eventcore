using EventCore.AggregateRoots;
using EventCore.Samples.Ecommerce.Domain.EmailBuilder.StateModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Domain.EmailBuilder
{
	public class EmailBuilderStateFactory : IAggregateRootStateFactory<EmailBuilderState>
	{
		private readonly AggregateRootStateBusinessEventResolver<EmailBuilderState> _resolver;
		private readonly IGenericBusinessEventHydrator _genericHydrator;

		// Gives caller the option of separating state databases by region.
		private readonly Func<string, EmailBuilderDbContext> _dbFactory;

		public EmailBuilderStateFactory(AggregateRootStateBusinessEventResolver<EmailBuilderState> resolver, IGenericBusinessEventHydrator genericHydrator, Func<string, EmailBuilderDbContext> dbFactory)
		{
			_resolver = resolver;
			_genericHydrator = genericHydrator;
			_dbFactory = dbFactory;
		}

		public Task<EmailBuilderState> CreateAndLoadToCheckpointAsync(string regionId, string context, string aggregateRootName, string aggregateRootId, CancellationToken cancellationToken)
		{
			// Ignores: context, agg root name and id.
			var state = new EmailBuilderState(_resolver, _genericHydrator, _dbFactory(regionId));
			return Task.FromResult(state);
		}
	}
}