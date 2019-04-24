using EventCore.AggregateRoots;
using EventCore.Samples.Ecommerce.Domain.EmailQueue.Commands;
using System;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Domain.EmailQueue
{
	public class EmailQueueAggregate : AggregateRoot<EmailQueueState>
	{
		public const string NAME = "EmailQueue";

		public EmailQueueAggregate(AggregateRootDependencies<EmailQueueState> dependencies) : base(dependencies, null, NAME)
		{
		}
	}
}
