using EventCore.AggregateRoots;
using EventCore.Samples.EmailSystem.Domain.SalesOrder;
using EventCore.Samples.EmailSystem.DomainApi.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.DomainApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SalesOrderController : ControllerBase
	{
		private readonly IAggregateRoot _ar;

		public SalesOrderController(IAggregateRoot ar)
		{
			_ar = ar;
		}

		// POST api/salesOrder/raiseSalesOrder
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public Task<IActionResult> RaiseSalesOrder([FromBody] SalesOrderCommand c) => CommandProcessor.ProcessCommandAsync(_ar, c);
	}
}
