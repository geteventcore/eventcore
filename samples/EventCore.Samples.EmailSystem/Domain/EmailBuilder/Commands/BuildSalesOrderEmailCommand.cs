using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.EmailSystem.Domain.EmailBuilder.Commands
{
	public class BuildSalesOrderEmailCommand : EmailBuilderCommand
	{
		public readonly string CustomerEmail;
		public readonly string SalesOrderId;
		public readonly decimal TotalPrice;

		public BuildSalesOrderEmailCommand(CommandMetadata metadata, Guid emailId, string customerEmail, string salesOrderId, decimal totalPrice) : base(metadata, emailId)
		{
			CustomerEmail = customerEmail;
			SalesOrderId = salesOrderId;
			TotalPrice = totalPrice;
		}
	}
}
