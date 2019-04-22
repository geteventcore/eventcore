using EventCore.Samples.EmailSystem.Domain.EmailBuilder.StateModels.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventCore.Samples.EmailSystem.Domain.EmailBuilder.StateModels.Config
{
	public class CausalIdDbModelConfig : IEntityTypeConfiguration<CausalIdHistoryDbModel>
	{
		public void Configure(EntityTypeBuilder<CausalIdHistoryDbModel> builder)
		{
			builder.HasKey(x => x.CausalId); // Expecting database to treat this as case insensitive.
		}
	}
}
