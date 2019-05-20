using EventCore.Samples.Ecommerce.Projections.SalesReport.SalesReportDb.DbModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Projections.SalesReport.SalesReportDb
{
	public class SalesReportDbContext : DbContext
	{
		public DbSet<SalesOrderDbModel> SalesOrder { get; set; }

		public SalesReportDbContext(DbContextOptions<SalesReportDbContext> options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.ApplyConfiguration(new SalesOrderDbModelConfig());
		}

		public bool ExistsSalesOrderId(string salesOrderId) => ExistsSalesOrderIdInternal(this, salesOrderId);

		private static Func<SalesReportDbContext, string, bool> ExistsSalesOrderIdInternal =
			EF.CompileQuery((SalesReportDbContext context, string salesOrderId) =>
				context.SalesOrder.Any(x => x.SalesOrderId == salesOrderId));
	}
}
