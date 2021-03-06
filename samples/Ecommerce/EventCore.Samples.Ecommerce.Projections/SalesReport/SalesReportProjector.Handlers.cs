﻿using EventCore.Projectors;
using EventCore.Samples.Ecommerce.Events;
using EventCore.Samples.Ecommerce.Projections.SalesReport.SalesReportDb.DbModels;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Projections.SalesReport
{
	public partial class SalesReportProjector :
		IHandleBusinessEvent<SalesOrderRaisedEvent>
	{
		public async Task HandleBusinessEventAsync(string streamId, long position, SalesOrderRaisedEvent e, CancellationToken cancellationToken)
		{
			using (var scope = _dbScopeFactory.Create())
			{
				if (!scope.Db.ExistsSalesOrderId(e.SalesOrderId))
				{
					scope.Db.SalesOrder.Add(new SalesOrderDbModel() { SalesOrderId = e.SalesOrderId, CustomerName = e.CustomerName, CustomerEmail = e.CustomerEmail, Price = e.Price });
					await scope.Db.SaveChangesAsync();
				}
			}
		}
	}
}
