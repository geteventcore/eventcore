using EventCore.Samples.Ecommerce.Domain.SalesOrder;
using EventCore.Samples.Ecommerce.Domain.SalesOrder.Commands;
using EventCore.Samples.Ecommerce.ServiceApi.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.ServiceApi.Areas.Domain.Controllers
{
	[Route("[area]/[controller]")]
	[ApiController]
	[Area("domain")]
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
		public Task<IActionResult> RaiseSalesOrder([FromBody] RaiseSalesOrderCommand c) => CommandProcessor.ProcessCommandAsync(_ar, c);
	}
}
