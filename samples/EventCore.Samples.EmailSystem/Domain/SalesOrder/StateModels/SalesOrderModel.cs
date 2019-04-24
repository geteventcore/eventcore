using EventCore.AggregateRoots;
using System;

namespace EventCore.Samples.EmailSystem.Domain.SalesOrder.StateModels
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
