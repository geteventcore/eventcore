using EventCore.AggregateRoots;
using EventCore.Samples.EmailSystem.Domain.SalesOrder.Commands;
using System;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain.SalesOrder
{
	public class SalesOrderAggregate : AggregateRoot<SalesOrderState>
	{
		public override bool SupportsSerializeableState {get => true;}
		
		public SalesOrderAggregate(AggregateRootDependencies<SalesOrderState> dependencies) : base(dependencies, null, "SalesOrder")
		{
		}
	}
}
