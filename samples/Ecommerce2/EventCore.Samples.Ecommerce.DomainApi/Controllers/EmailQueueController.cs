using EventCore.Samples.Ecommerce.Domain.EmailQueue;
using EventCore.Samples.Ecommerce.Domain.EmailQueue.Commands;
using EventCore.Samples.Ecommerce.DomainApi.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.DomainApi.Controllers
{
	[Route("[controller]")]
	[ApiController]
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
