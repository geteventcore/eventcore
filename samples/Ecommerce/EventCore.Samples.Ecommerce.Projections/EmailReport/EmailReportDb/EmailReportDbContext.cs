using EventCore.Samples.Ecommerce.Projections.EmailReport.EmailReportDb.DbModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Projections.EmailReport.EmailReportDb
{
	public class EmailReportDbContext : DbContext
	{
		public DbSet<EmailDbModel> Email { get; set; }

		public EmailReportDbContext(DbContextOptions<EmailReportDbContext> options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.ApplyConfiguration(new EmailDbModelConfig());
		}

		public bool ExistsEmailId(Guid emailId) => ExistsEmailIdInternal(this, emailId);

		private static Func<EmailReportDbContext, Guid, bool> ExistsEmailIdInternal =
			EF.CompileQuery((EmailReportDbContext context, Guid emailId) =>
				context.Email.Any(x => x.EmailId == emailId));
	}
}
