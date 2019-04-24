using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.Ecommerce.Domain.SalesOrder.StateModels
{
	public class SalesOrderModel
	{
		public readonly Guid EmailId;
		
		public SalesOrderModel(Guid emailId)
		{
			EmailId = emailId;
		}
	}
}
