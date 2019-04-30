using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.Ecommerce.Domain.EmailBuilder.Commands
{
	public class BuildSalesOrderEmailCommand : EmailBuilderCommand
	{
		public readonly string CustomerEmail;
		public readonly string SalesOrderId;
		public readonly decimal TotalPrice;

		public BuildSalesOrderEmailCommand(DomainCommandMetadata _metadata, Guid emailId, string customerEmail, string salesOrderId, decimal totalPrice) : base(_metadata, emailId)
		{
			CustomerEmail = customerEmail;
			SalesOrderId = salesOrderId;
			TotalPrice = totalPrice;
		}
	}
}
