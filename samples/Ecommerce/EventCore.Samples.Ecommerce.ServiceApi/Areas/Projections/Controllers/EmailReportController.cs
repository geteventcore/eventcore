using EventCore.Samples.Ecommerce.Projections.EmailReport.EmailReportDb;
using EventCore.Samples.Ecommerce.Projections.EmailReport.EmailReportDb.DbModels;
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
		private readonly EmailReportDbContext _db;

		public EmailReportController(EmailReportDbContext db)
		{
			_db = db;
		}

		[HttpGet("Emails")]
		// [ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IEnumerable<EmailDbModel>> EmailsAsync() => await _db.Email.ToListAsync();
	}
}
