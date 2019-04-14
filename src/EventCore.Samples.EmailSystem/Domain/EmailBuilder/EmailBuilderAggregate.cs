using EventCore.AggregateRoots;
using EventCore.Samples.EmailSystem.Domain.EmailBuilder.Commands;
using System;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain.EmailBuilder
{
	public class EmailBuilderAggregate : AggregateRoot<EmailBuilderState>
	{
		public const string NAME = "EmailBuilder";

		public override bool SupportsSerializeableState {get => true;}
		
		public EmailBuilderAggregate(AggregateRootDependencies<EmailBuilderState> dependencies) : base(dependencies, null, NAME)
		{
		}
	}
}
