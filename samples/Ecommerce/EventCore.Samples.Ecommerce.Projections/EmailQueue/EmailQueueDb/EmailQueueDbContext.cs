using EventCore.Samples.Ecommerce.Projections.EmailQueue.EmailQueueDb.DbModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Projections.EmailQueue.EmailQueueDb
{
	public class EmailQueueDbContext : DbContext
	{
		public DbSet<EmailDbModel> Email { get; set; }

		public EmailQueueDbContext(DbContextOptions<EmailQueueDbContext> options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.ApplyConfiguration(new EmailDbModelConfig());
		}

		public bool ExistsEmailId(Guid emailId) => ExistsEmailIdInternal(this, emailId);

		private static Func<EmailQueueDbContext, Guid, bool> ExistsEmailIdInternal =
			EF.CompileQuery((EmailQueueDbContext context, Guid emailId) =>
				context.Email.Any(x => x.EmailId == emailId));
	}
}
