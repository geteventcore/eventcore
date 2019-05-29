using EventCore.AggregateRoots;
using EventCore.EventSourcing;
using EventCore.Samples.Ecommerce.Events;
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

			var e = new SalesOrderRaisedEvent(
				BusinessEventMetadata.FromCausalId(c.GetCommandId()),
				c.SalesOrderId, c.CustomerName, c.CustomerEmail, c.Price
			);
			return CommandResult.FromEventIAsync(e);
		}
	}
}
