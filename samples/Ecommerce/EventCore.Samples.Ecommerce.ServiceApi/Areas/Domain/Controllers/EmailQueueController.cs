using EventCore.Samples.Ecommerce.Domain.EmailQueue;
using EventCore.Samples.Ecommerce.Domain.EmailQueue.Commands;
using EventCore.Samples.Ecommerce.ServiceApi.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.ServiceApi.Areas.Domain.Controllers
{
	[Route("[area]/[controller]")]
	[ApiController]
	[Area("domain")]
	public class EmailQueueController : ControllerBase
	{
		private readonly EmailQueueAggregate _ar;

		public EmailQueueController(EmailQueueAggregate ar)
		{
			_ar = ar;
		}

		[HttpPost("EnqueueEmail")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public Task<IActionResult> EnqueueEmailAsync([FromBody] EnqueueEmailCommand c) => CommandProcessor.ProcessCommandAsync(_ar, c);
	}
}
