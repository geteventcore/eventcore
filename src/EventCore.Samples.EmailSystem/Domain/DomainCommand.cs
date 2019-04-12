using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.EmailSystem.Domain
{
	public abstract class DomainCommand : Command
	{
		public DomainCommand(CommandMetadata metadata) : base(metadata)
		{
		}
	}
}
