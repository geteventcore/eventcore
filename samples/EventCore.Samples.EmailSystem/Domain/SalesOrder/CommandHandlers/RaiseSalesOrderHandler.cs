using EventCore.AggregateRoots;
using EventCore.EventSourcing;
using EventCore.Samples.EmailSystem.Domain.SalesOrder.Commands;
using EventCore.Samples.EmailSystem.Events;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain.SalesOrder.CommandHandlers
{
	public class RaiseSalesOrderHandler : SalesOrderCommandHandler<RaiseSalesOrderCommand>
	{
		public override Task<ICommandValidationResult> ValidateForStateAsync(SalesOrderState state, RaiseSalesOrderCommand c)
		{
			if (state.SalesOrder != null) return CommandValidationResult.FromErrorAsync("Duplicate sales order id.");
			else return CommandValidationResult.FromValidAsync();
		}

		public override Task<ICommandEventsResult> ProcessCommandAsync(SalesOrderState state, RaiseSalesOrderCommand c)
		{
			var e = new SalesOrderRaisedEvent(
				BusinessEventMetadata.FromCausalId(c._Metadata.CommandId),
				c.SalesOrderId, c.CustomerName, c.CustomerEmail, c.TotalPrice
				);
			return CommandEventsResult.FromEventAsync(e);
		}
	}
}
