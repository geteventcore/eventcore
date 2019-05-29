using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.Ecommerce.Domain.SalesOrder.Commands
{
	public class RaiseSalesOrderCommand : SalesOrderCommand
	{
		public readonly string CustomerName;
		public readonly string CustomerEmail;
		public readonly decimal Price;

		public RaiseSalesOrderCommand(CommandMetadata _metadata, string salesOrderId, string customerName, string customerEmail, decimal price) : base(_metadata, salesOrderId)
		{
			CustomerName = customerName;
			CustomerEmail = customerEmail;
			Price = price;
		}
	}
}
