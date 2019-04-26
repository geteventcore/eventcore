using EventCore.AggregateRoots;
using EventCore.EventSourcing;
using EventCore.Samples.Ecommerce.Domain.SalesOrder.Commands;
using EventCore.Samples.Ecommerce.Domain.Events;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Domain.SalesOrder.CommandHandlers
{
	public class RaiseSalesOrderHandler : SalesOrderCommandHandler<RaiseSalesOrderCommand>
	{
		public override Task<ICommandValidationResult> ValidateStateAsync(SalesOrderState state, RaiseSalesOrderCommand c)
		{
			if (state.SalesOrder != null) return CommandValidationResult.FromErrorIAsync("Duplicate sales order id.");
			else return CommandValidationResult.FromValidIAsync();
		}

		public override Task<ICommandEventsResult> ProcessCommandAsync(SalesOrderState state, RaiseSalesOrderCommand c)
		{
			var e = new SalesOrderRaisedEvent(
				BusinessEventMetadata.FromCausalId(c._Metadata.CommandId),
				c.SalesOrderId, c.CustomerName, c.CustomerEmail, c.TotalPrice
				);
			return CommandEventsResult.FromEventIAsync(e);
		}
	}
}
