using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.Ecommerce.Domain.SalesOrder
{
	public abstract class SalesOrderCommand : DomainCommand
	{
		public readonly string SalesOrderId;

		public SalesOrderCommand(CommandMetadata _metadata, string salesOrderId) : base(_metadata)
		{
			SalesOrderId = salesOrderId;
		}

		public override string GetAggregateRootName() => SalesOrderAggregate.NAME;
		public override string GetAggregateRootId() => SalesOrderId;
	}
}
