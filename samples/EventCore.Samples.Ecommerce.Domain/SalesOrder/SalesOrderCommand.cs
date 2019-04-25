using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.Ecommerce.Domain.SalesOrder
{
	public abstract class SalesOrderCommand : DomainCommand
	{
		public override string _AggregateRootName { get => SalesOrderAggregate.NAME; }

		public readonly string SalesOrderId;

		public SalesOrderCommand(ICommandMetadata _metadata, string salesOrderId) : base(_metadata)
		{
			SalesOrderId = salesOrderId;
		}
		
		public override string GetAggregateRootId() => SalesOrderId;
	}
}
