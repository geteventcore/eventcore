using EventCore.Samples.EmailSystem.Domain.SalesOrder;
using EventCore.Samples.EmailSystem.Domain.SalesOrder.Commands;
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
		private readonly SalesOrderAggregate _ar;

		public SalesOrderController(SalesOrderAggregate ar)
		{
			_ar = ar;
		}

		// POST api/salesOrder/raiseSalesOrder
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public Task<IActionResult> Post([FromBody] SalesOrderCommand c) => CommandProcessor.ProcessCommandAsync(_ar, c);
	}
}
