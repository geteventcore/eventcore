using EventCore.AggregateRoots;
using EventCore.Samples.Ecommerce.Domain.SalesOrder.Commands;
using System;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Domain.SalesOrder
{
	public class SalesOrderAggregate : AggregateRoot<SalesOrderState>
	{
		public const string NAME = "SalesOrder";

		public SalesOrderAggregate(AggregateRootDependencies<SalesOrderState> dependencies) : base(dependencies, null, NAME)
		{
		}
	}
}
