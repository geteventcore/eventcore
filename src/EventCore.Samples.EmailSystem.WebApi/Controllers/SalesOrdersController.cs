using EventCore.Samples.EmailSystem.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.WebApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SalesOrdersController : ControllerBase
	{
		// GET api/salesOrders
		[HttpGet]
		public async Task<ActionResult<IEnumerable<SalesOrder>>> Get()
		{
			await Task.Delay(10);
			return new List<SalesOrder>();
		}

		// GET api/salesOrders/so123
		[HttpGet("{id}")]
		public async Task<IActionResult> Get(string salesOrderId)
		{
			await Task.Delay(10);
			return null;
		}

		// POST api/salesOrders
		[HttpPost]
		public async Task<IActionResult> Post(string salesOrderId, [FromBody] SalesOrderPost salesOrder)
		{
			await Task.Delay(10);
			return Ok();
		}
	}
}
