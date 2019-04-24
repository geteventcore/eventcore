using EventCore.Samples.Ecommerce.Domain.SalesOrder;
using EventCore.Samples.Ecommerce.DomainApi.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.DomainApi.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class SalesOrderController : ControllerBase
	{
		private readonly SalesOrderAggregate _ar;

		public SalesOrderController(SalesOrderAggregate ar)
		{
			_ar = ar;
		}

		[HttpPost("RaiseSalesOrder")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public Task<IActionResult> RaiseSalesOrder([FromBody] SalesOrderCommand c) => CommandProcessor.ProcessCommandAsync(_ar, c);
	}
}
