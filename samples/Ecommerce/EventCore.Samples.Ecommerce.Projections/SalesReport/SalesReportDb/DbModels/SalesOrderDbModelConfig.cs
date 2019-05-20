using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace EventCore.Samples.Ecommerce.Projections.SalesReport.SalesReportDb.DbModels
{
	public class SalesOrderDbModelConfig : IEntityTypeConfiguration<SalesOrderDbModel>
	{
		public void Configure(EntityTypeBuilder<SalesOrderDbModel> builder)
		{
			builder.HasKey(x => x.SalesOrderId);
		}
	}
}
