using EventCore.Projectors;
using EventCore.Samples.Ecommerce.Domain.Events;
using EventCore.Samples.Ecommerce.Projections.SalesReport.SalesReportDb.DbModels;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Projections.SalesReport
{
	public partial class SalesReportProjector :
		IHandleBusinessEvent<SalesOrderRaisedEvent>
	{
		private static int _counter = 0;
		public async Task HandleBusinessEventAsync(string streamId, long position, SalesOrderRaisedEvent e, CancellationToken cancellationToken)
		{
			using (var scope = _dbScopeFactory.Create())
			{
				if (!scope.Db.ExistsSalesOrderId(e.SalesOrderId))
				{
					scope.Db.SalesOrder.Add(new SalesOrderDbModel() { SalesOrderId = e.SalesOrderId });
					await scope.Db.SaveChangesAsync();
				}
			}
		}
	}
}
