using EventCore.Samples.SimpleEventStore.StreamDb.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventCore.Samples.SimpleEventStore.StreamDb.Config
{
	public class StreamEventDbModelConfig : IEntityTypeConfiguration<StreamEventDbModel>
	{
		public void Configure(EntityTypeBuilder<StreamEventDbModel> builder)
		{
			builder.HasKey(x => x.GlobalPosition);
		}
	}
}
