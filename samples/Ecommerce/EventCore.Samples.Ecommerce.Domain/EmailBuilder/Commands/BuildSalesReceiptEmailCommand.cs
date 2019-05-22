using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.Ecommerce.Domain.EmailBuilder.Commands
{
	public class BuildSalesReceiptEmailCommand : EmailBuilderCommand
	{
		public readonly string CustomerEmail;
		public readonly string SalesOrderId;
		public readonly decimal TotalPrice;

		public BuildSalesReceiptEmailCommand(CommandMetadata _metadata, Guid emailId, string customerEmail, string salesOrderId, decimal totalPrice) : base(_metadata, emailId)
		{
			CustomerEmail = customerEmail;
			SalesOrderId = salesOrderId;
			TotalPrice = totalPrice;
		}
	}
}
