using EventCore.AggregateRoots;
using EventCore.Samples.EmailSystem.Domain.EmailQueue.Commands;
using System;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain.EmailQueue
{
	public class EmailQueueAggregate : AggregateRoot<EmailQueueState>
	{
		public const string NAME = "EmailQueue";

		public override bool SupportsSerializeableState {get => true;}
		
		public EmailQueueAggregate(AggregateRootDependencies<EmailQueueState> dependencies) : base(dependencies, null, NAME)
		{
		}
	}
}
