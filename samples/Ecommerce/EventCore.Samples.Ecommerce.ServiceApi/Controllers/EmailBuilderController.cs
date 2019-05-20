using EventCore.Samples.Ecommerce.Domain.EmailBuilder;
using EventCore.Samples.Ecommerce.Domain.EmailBuilder.Commands;
using EventCore.Samples.Ecommerce.ServiceApi.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.ServiceApi.Controllers
{
	[Route("domain/[controller]")]
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
