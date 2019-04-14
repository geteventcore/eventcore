using EventCore.Samples.EmailSystem.Domain.EmailBuilder;
using EventCore.Samples.EmailSystem.Domain.EmailBuilder.Commands;
using EventCore.Samples.EmailSystem.DomainApi.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.DomainApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class EmailBuilderController : ControllerBase
	{
		private readonly EmailBuilderAggregate _ar;

		public EmailBuilderController(EmailBuilderAggregate ar)
		{
			_ar = ar;
		}

		// POST api/emailBuilder/buildSalesOrderEmail
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public Task<IActionResult> Post([FromBody] BuildSalesOrderEmailCommand c) => CommandProcessor.ProcessCommandAsync(_ar, c);
	}
}
