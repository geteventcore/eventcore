﻿namespace EventCore.Samples.Ecommerce.PublicApi.Models
{
	public class SalesOrder
	{
		public readonly string SalesOrderId;
		public readonly string CustomerName;
		public readonly string CustomerEmail;
		public readonly decimal TotalPrice;

		public SalesOrder(string salesOrderId, string customerName, string customerEmail, decimal totalPrice)
		{
			SalesOrderId = salesOrderId;
			CustomerName = customerName;
			CustomerEmail = customerEmail;
			TotalPrice = totalPrice;
		}
	}
}
