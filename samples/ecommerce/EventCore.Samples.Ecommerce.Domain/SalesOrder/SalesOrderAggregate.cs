using EventCore.AggregateRoots;
using EventCore.Samples.Ecommerce.Domain.SalesOrder.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Domain.SalesOrder
{
	public class SalesOrderAggregate : AggregateRoot<SalesOrderState>,
		IHandleCommand<SalesOrderState, RaiseSalesOrderCommand>
	{
		public const string NAME = "SalesOrder";

		public SalesOrderAggregate(AggregateRootDependencies<SalesOrderState> dependencies) : base(dependencies, null, NAME)
		{
		}

		public Task<ICommandResult> HandleCommandAsync(SalesOrderState s, RaiseSalesOrderCommand c, CancellationToken ct)
		{
			if (s.SalesOrder != null) return CommandResult.FromErrorIAsync("Duplicate sales order id.");

			throw new NotImplementedException();
		}
	}
}
