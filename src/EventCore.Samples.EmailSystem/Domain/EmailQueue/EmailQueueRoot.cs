using EventCore.AggregateRoots;
using EventCore.Samples.EmailSystem.Domain.EmailQueue.Commands;
using System;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain.EmailQueue
{
	public class EmailQueueRoot : AggregateRoot<EmailQueueState>
	{

		public EmailQueueRoot(AggregateRootDependencies<EmailQueueState> dependencies) : base(dependencies, null, "EmailQueue")
		{
		}
	}
}
