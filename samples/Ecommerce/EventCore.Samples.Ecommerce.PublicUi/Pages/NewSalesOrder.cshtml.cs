using EventCore.AggregateRoots;
using EventCore.Samples.Ecommerce.Domain.Clients;
using EventCore.Samples.Ecommerce.Domain.SalesOrder.Commands;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.PublicUi.Pages
{
	public class NewSalesOrderModel : PageModel
	{
		private readonly SalesOrderClient _salesOrderClient;

		public NewSalesOrderModel(SalesOrderClient salesOrderClient)
		{
			_salesOrderClient = salesOrderClient;
		}

		[BindProperty]
		public string CustomerName { get; set; }

		[BindProperty]
		public string CustomerEmail { get; set; }

		[BindProperty]
		public decimal TotalPrice { get; set; }

		public async Task<IActionResult> OnPostAsync()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			await _salesOrderClient.ExecuteAsync(
				new RaiseSalesOrderCommand(CommandMetadata.Default, Guid.NewGuid().ToString(), CustomerName, CustomerEmail, TotalPrice)
				);

			return RedirectToPage("/Index", new { rand = new Random().Next(1, int.MaxValue) }); // Random number to force refresh.
		}
	}
}