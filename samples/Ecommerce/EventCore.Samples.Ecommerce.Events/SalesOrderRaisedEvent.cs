using EventCore.EventSourcing;
using System;

namespace EventCore.Samples.Ecommerce.Events
{
	public class SalesOrderRaisedEvent : BusinessEvent
	{
		public readonly string SalesOrderId;
		public readonly string CustomerName;
		public readonly string CustomerEmail;
		public readonly decimal Price;

		public SalesOrderRaisedEvent(BusinessEventMetadata _metadata, string salesOrderId, string customerName, string customerEmail, decimal price) : base(_metadata)
		{
			SalesOrderId = salesOrderId;
			CustomerName = customerName;
			CustomerEmail = customerEmail;
			Price = price;
		}
	}
}
