using EventCore.Samples.Ecommerce.Projections.EmailQueue.EmailQueueDb;
using EventCore.Samples.Ecommerce.Projections.EmailQueue.EmailQueueDb.DbModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.ServiceApi.Areas.Projections.Controllers
{
	[Route("[area]/[controller]")]
	[ApiController]
	[Area("projections")]
	public class EmailReportController : ControllerBase
	{
		private readonly EmailQueueDbContext _db;

		public EmailReportController(EmailQueueDbContext db)
		{
			_db = db;
		}

		[HttpGet("Emails")]
		// [ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IEnumerable<EmailDbModel>> EmailsAsync() => await _db.Email.ToListAsync();
	}
}
