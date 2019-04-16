using EventCore.AggregateRoots;
using EventCore.Samples.EmailSystem.Domain.EmailQueue.Commands;
using EventCore.Samples.EmailSystem.DomainApi.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.DomainApi.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class EmailQueueController : ControllerBase
	{
		private readonly IAggregateRoot _ar;

		public EmailQueueController(IAggregateRoot ar)
		{
			_ar = ar;
		}

		[HttpPost("EnqueueEmail")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public Task<IActionResult> EnqueueEmailAsync([FromBody] EnqueueEmailCommand c) => CommandProcessor.ProcessCommandAsync(_ar, c);
	}
}
