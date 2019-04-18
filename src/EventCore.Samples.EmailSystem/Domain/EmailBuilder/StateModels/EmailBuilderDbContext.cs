using EventCore.AggregateRoots.EntityFrameworkState;
using EventCore.Samples.EmailSystem.Domain.EmailBuilder.StateModels.Config;
using EventCore.Samples.EmailSystem.Domain.EmailBuilder.StateModels.DbModels;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain.EmailBuilder.StateModels
{
	public class EmailBuilderDbContext : DbContext, IStoreCausalIdHistory
	{
		public DbSet<EmailDbModel> Email { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.ApplyConfiguration(new EmailDbModelConfig());
		}

		public Task AddCausalIdToHistoryIfNotExistsAsync(string causalId)
		{
			throw new System.NotImplementedException();
		}

		public Task<bool> ExistsCausalIdInHistoryAsync(string causalId)
		{
			throw new System.NotImplementedException();
		}
	}
}
