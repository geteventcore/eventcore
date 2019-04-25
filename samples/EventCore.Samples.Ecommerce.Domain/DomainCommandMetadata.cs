using EventCore.AggregateRoots;

namespace EventCore.Samples.Ecommerce.Domain
{
	public class DomainCommandMetadata : CommandMetadata
	{
		public DomainCommandMetadata(string commandId) : base(commandId)
		{
		}
	}
}
