using System;

namespace EventCore.Samples.Ecommerce.Projections.SalesReport.SalesReportDb.DbModels
{
	public class SalesOrderDbModel
	{
		public string SalesOrderId { get; set; }
		public string CustomerName { get; set; }
		public string CustomerEmail { get; set; }
		public decimal TotalPrice { get; set; }
	}
}
