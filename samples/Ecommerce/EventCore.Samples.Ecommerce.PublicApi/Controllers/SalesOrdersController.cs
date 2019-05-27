using EventCore.AggregateRoots;
using EventCore.Samples.Ecommerce.Domain.Clients;
using EventCore.Samples.Ecommerce.Domain.SalesOrder.Commands;
using EventCore.Samples.Ecommerce.Projections.SalesReport.SalesReportDb;
using EventCore.Samples.Ecommerce.PublicApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.PublicApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SalesOrdersController : ControllerBase
	{
		private readonly SalesReportDbContext _db;
		private readonly SalesOrderClient _salesOrderClient;

		public SalesOrdersController(SalesReportDbContext db, SalesOrderClient salesOrderClient)
		{
			_db = db;
			_salesOrderClient = salesOrderClient;
		}

		// GET api/salesOrders
		[HttpGet]
		public async Task<IEnumerable<SalesOrder>> GetSalesOrders()
		{
			return await _db.SalesOrder.Select(x => new SalesOrder(x.SalesOrderId, x.CustomerName, x.CustomerEmail, x.TotalPrice)).ToListAsync();
		}

		// GET api/salesOrders/so123
		[HttpGet("{salesOrderId}")]
		public async Task<IActionResult> GetSalesOrder(string salesOrderId)
		{
			await Task.Delay(10);
			return null;
		}

		// GET api/salesOrders/total
		[HttpGet("total")]
		public IActionResult GetTotal()
		{
			return Ok(_db.SalesOrder.Sum(x => x.TotalPrice));
		}

		// POST api/salesOrders/so123
		[HttpPost("{salesOrderId}")]
		public async Task<IActionResult> PostNewSalesOrder(string salesOrderId, [FromBody] NewSalesOrder so)
		{
			await _salesOrderClient.ExecuteAsync(
				new RaiseSalesOrderCommand(CommandMetadata.Default, salesOrderId, so.CustomerName, so.CustomerEmail, so.TotalPrice)
				);
			return Ok();
		}
	}
}
