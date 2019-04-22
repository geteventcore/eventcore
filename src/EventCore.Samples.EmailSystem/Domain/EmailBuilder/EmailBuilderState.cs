using EventCore.AggregateRoots;
using EventCore.AggregateRoots.EntityFrameworkState;
using EventCore.Samples.EmailSystem.Domain.EmailBuilder.StateModels;
using EventCore.Samples.EmailSystem.Events;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain.EmailBuilder
{
	public class EmailBuilderState : DbContextAggregateRootState<EmailBuilderDbContext>,
		IApplyBusinessEvent<EmailEnqueuedEvent>
	{
		public EmailBuilderState(AggregateRootStateBusinessEventResolver<EmailBuilderState> resolver, EmailBuilderDbContext db) : base(resolver, db)
		{
		}

		public Task ApplyBusinessEventAsync(string streamId, long position, EmailEnqueuedEvent e, CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
