using EventCore.Samples.SimpleEventStore.EventStoreDb.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventCore.Samples.SimpleEventStore.EventStoreDb.Config
{
	public class SubscriptionFilterDbModelConfig : IEntityTypeConfiguration<SubscriptionFilterDbModel>
	{
		public void Configure(EntityTypeBuilder<SubscriptionFilterDbModel> builder)
		{
			builder.HasKey(x => new { x.SubscriptionName, x.StreamIdPrefix });
		}
	}
}
