using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.EmailSystem.Domain.SalesOrder
{
	public abstract class SalesOrderCommand : DomainCommand
	{
		public override string _AggregateRootName { get => SalesOrderAggregate.NAME; }

		public readonly string SalesOrderId;

		public SalesOrderCommand(CommandMetadata metadata, string salesOrderId) : base(metadata)
		{
			SalesOrderId = salesOrderId;
		}

		public override string RegionId() => null;
		public override string AggregateRootId() => SalesOrderId;
	}
}
