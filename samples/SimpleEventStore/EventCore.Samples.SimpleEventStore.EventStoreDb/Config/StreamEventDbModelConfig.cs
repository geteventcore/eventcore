using EventCore.Samples.SimpleEventStore.EventStoreDb.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventCore.Samples.SimpleEventStore.EventStoreDb.Config
{
	public class StreamEventDbModelConfig : IEntityTypeConfiguration<StreamEventDbModel>
	{
		public void Configure(EntityTypeBuilder<StreamEventDbModel> builder)
		{
			builder.HasKey(x => x.GlobalPosition);
		}
	}
}
