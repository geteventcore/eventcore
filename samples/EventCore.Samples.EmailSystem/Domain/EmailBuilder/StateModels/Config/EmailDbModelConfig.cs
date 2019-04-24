using EventCore.Samples.EmailSystem.Domain.EmailBuilder.StateModels.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace EventCore.Samples.EmailSystem.Domain.EmailBuilder.StateModels.Config
{
	public class EmailDbModelConfig : IEntityTypeConfiguration<EmailDbModel>
	{
		public void Configure(EntityTypeBuilder<EmailDbModel> builder)
		{
			builder.HasKey(x => x.EmailId);
		}
	}
}
