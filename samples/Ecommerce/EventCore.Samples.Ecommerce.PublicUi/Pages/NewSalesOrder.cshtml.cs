using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.PublicUi.Pages
{
	public class NewSalesOrderModel : PageModel
	{
		public NewSalesOrderModel()
		{
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

			await Task.Delay(10);
			System.Console.WriteLine($"Raising new sales order: {CustomerName} {CustomerEmail} {TotalPrice}");

			return RedirectToPage("/Index");
		}
	}
}