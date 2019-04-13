using EventCore.Samples.EmailSystem.Domain.EmailQueue;
using EventCore.Samples.EmailSystem.Domain.EmailQueue.Commands;
using EventCore.Samples.EmailSystem.DomainApi.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.DomainApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class EmailQueueController : ControllerBase
	{
		private readonly IAggregateRootHarness<EmailQueueRoot, EmailQueueState> _ar;

		public EmailQueueController(IAggregateRootHarness<EmailQueueRoot, EmailQueueState> ar)
		{
		}

		// POST api/emailQueue
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Post([FromBody] EnqueueEmailCommand c)
		{
			var result = await _ar.HandleCommandAsync(c);

			if(result.IsSuccess) return Ok();
			else return BadRequest(result.ValidationErrors);
		}
	}
}
