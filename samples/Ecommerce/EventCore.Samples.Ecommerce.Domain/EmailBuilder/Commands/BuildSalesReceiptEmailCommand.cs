using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.Ecommerce.Domain.EmailBuilder.Commands
{
	public class BuildSalesReceiptEmailCommand : EmailBuilderCommand
	{
		public readonly string CustomerEmail;
		public readonly string SalesOrderId;
		public readonly decimal Price;

		public BuildSalesReceiptEmailCommand(CommandMetadata _metadata, Guid emailId, string customerEmail, string salesOrderId, decimal price) : base(_metadata, emailId)
		{
			CustomerEmail = customerEmail;
			SalesOrderId = salesOrderId;
			Price = price;
		}
	}
}
