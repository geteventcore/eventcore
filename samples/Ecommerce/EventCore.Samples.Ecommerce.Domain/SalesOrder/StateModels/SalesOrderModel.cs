using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.Ecommerce.Domain.SalesOrder.StateModels
{
	public class SalesOrderModel
	{
		public readonly string SalesOrderId;
		
		public SalesOrderModel(string salesOrderId)
		{
			SalesOrderId = salesOrderId;
		}
	}
}
