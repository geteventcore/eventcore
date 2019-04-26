using EventCore.EventSourcing;
using System;

namespace EventCore.Samples.Ecommerce.Domain.Events
{
	public class SalesOrderRaisedEvent : BusinessEvent
	{
		public readonly string SalesOrderId;
		public readonly string CustomerName;
		public readonly string CustomerEmail;
		public readonly decimal TotalPrice;

		public SalesOrderRaisedEvent(BusinessEventMetadata metadata, string salesOrderId, string customerName, string customerEmail, decimal totalPrice) : base(metadata)
		{
			SalesOrderId = salesOrderId;
			CustomerName = customerName;
			CustomerEmail = customerEmail;
			TotalPrice = totalPrice;
		}
	}
}
