using EventCore.AggregateRoots;
using EventCore.Samples.Ecommerce.Domain.EmailBuilder.Commands;
using System;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Domain.EmailBuilder
{
	public class EmailBuilderAggregate : AggregateRoot<EmailBuilderState>
	{
		public const string NAME = "EmailBuilder";

		public EmailBuilderAggregate(AggregateRootDependencies<EmailBuilderState> dependencies) : base(dependencies, null, NAME)
		{
		}
	}
}
