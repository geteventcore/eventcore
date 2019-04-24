using EventCore.Samples.EmailSystem.Domain.EmailBuilder;
using EventCore.Samples.EmailSystem.Domain.EmailBuilder.Commands;
using EventCore.Samples.EmailSystem.DomainApi.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.DomainApi.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class EmailBuilderController : ControllerBase
	{
		private readonly EmailBuilderAggregate _ar;

		public EmailBuilderController(EmailBuilderAggregate ar)
		{
			_ar = ar;
		}

		[HttpPost("BuildSalesOrderEmail")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public Task<IActionResult> BuildSalesOrderEmailAsync([FromBody] BuildSalesOrderEmailCommand c) => CommandProcessor.ProcessCommandAsync(_ar, c);
	}
}
