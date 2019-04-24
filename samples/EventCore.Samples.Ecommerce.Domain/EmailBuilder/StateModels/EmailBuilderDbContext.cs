using EventCore.Samples.Ecommerce.Domain.EmailBuilder.StateModels.Config;
using EventCore.Samples.Ecommerce.Domain.EmailBuilder.StateModels.DbModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Domain.EmailBuilder.StateModels
{
	public class EmailBuilderDbContext : DbContext
	{
		public DbSet<CausalIdHistoryDbModel> CausalIdHistory { get; set; }
		public DbSet<EmailDbModel> Email { get; set; }

		public EmailBuilderDbContext(DbContextOptions<EmailBuilderDbContext> options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.ApplyConfiguration(new EmailDbModelConfig());
		}

		public async Task AddCausalIdToHistoryIfNotExistsAsync(string causalId)
		{
			if (await GetCausalIdHistoryOrDefaultAsync(this, causalId) == null)
			{
				CausalIdHistory.Add(new CausalIdHistoryDbModel() { CausalId = causalId });
				await SaveChangesAsync();
			}
		}

		public async Task<bool> ExistsCausalIdInHistoryAsync(string causalId) => (await GetCausalIdHistoryOrDefaultAsync(this, causalId)) != null;

		private static Func<EmailBuilderDbContext, string, Task<CausalIdHistoryDbModel>> GetCausalIdHistoryOrDefaultAsync =
			EF.CompileAsyncQuery((EmailBuilderDbContext context, string causalId) =>
				context.CausalIdHistory
					.Where(x => x.CausalId == causalId) // No string transformation (i.e. ToUpper) assuming db is case insensitive by default.
					.FirstOrDefault());
	}
}
