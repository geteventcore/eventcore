using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.Ecommerce.Domain.SalesOrder.Commands
{
	public class RaiseSalesOrderCommand : SalesOrderCommand
	{
		public readonly string CustomerName;
		public readonly string CustomerEmail;
		public readonly decimal TotalPrice;

		public RaiseSalesOrderCommand(DomainCommandMetadata metadata, string salesOrderId, string customerName, string customerEmail, decimal totalPrice) : base(metadata, salesOrderId)
		{
			CustomerName = customerName;
			CustomerEmail = customerEmail;
			TotalPrice = totalPrice;
		}
	}
}
