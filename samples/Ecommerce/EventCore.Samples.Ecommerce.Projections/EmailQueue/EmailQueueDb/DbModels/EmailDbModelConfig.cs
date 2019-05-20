using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace EventCore.Samples.Ecommerce.Projections.EmailQueue.EmailQueueDb.DbModels
{
	public class EmailDbModelConfig : IEntityTypeConfiguration<EmailDbModel>
	{
		public void Configure(EntityTypeBuilder<EmailDbModel> builder)
		{
			builder.HasKey(x => x.EmailId);
		}
	}
}
