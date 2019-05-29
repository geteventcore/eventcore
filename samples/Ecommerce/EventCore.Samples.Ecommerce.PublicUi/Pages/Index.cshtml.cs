using EventCore.Samples.Ecommerce.Projections.SalesReport.SalesReportDb;
using EventCore.Samples.Ecommerce.Projections.SalesReport.SalesReportDb.DbModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.PublicUi.Pages
{
	public class SalesReportModel : PageModel
	{
		public IList<SalesOrderDbModel> SalesOrders { get; private set; }
		public decimal Total { get; private set; }

		private readonly SalesReportDbContext _db;

		public SalesReportModel(SalesReportDbContext db)
		{
			_db = db;
		}

		public async Task OnGetAsync()
		{
			SalesOrders = await _db.SalesOrder.AsNoTracking().ToListAsync();
			Total = await _db.SalesOrder.SumAsync(x => x.Price);
		}
	}
}
