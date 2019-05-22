using EventCore.Projectors;
using EventCore.Samples.Ecommerce.Events;
using EventCore.Samples.Ecommerce.Projections.EmailReport.EmailReportDb.DbModels;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Projections.EmailReport
{
	public partial class EmailReportProjector :
		IHandleBusinessEvent<EmailEnqueuedEvent>
	{
		public async Task HandleBusinessEventAsync(string streamId, long position, EmailEnqueuedEvent e, CancellationToken cancellationToken)
		{
			using (var scope = _dbScopeFactory.Create())
			{
				if (!scope.Db.ExistsEmailId(e.EmailId))
				{
					scope.Db.Email.Add(new EmailDbModel() { EmailId = e.EmailId });
					await scope.Db.SaveChangesAsync();
				}
			}
		}
	}
}
