using EventCore.ProcessManagers;
using EventCore.Samples.Ecommerce.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.ProcessManagers.EmailManagement
{
	public class ProcessManagerStateRepo : IProcessManagerStateRepo
	{
		private readonly IDbContextScopeFactory<ProcessManagerStateDbContext> _dbScopeFactory;

		public async Task AddOrUpdateQueuedProcessAsync(string processType, string processId, DateTime dueUtc)
		{
			using (var dbScope = _dbScopeFactory.Create())
			{
				var queuedProcess = dbScope.Db.GetQueuedProcess(processType, processId);
				if (queuedProcess == null)
				{
					dbScope.Db.QueuedProcess.Add(new QueuedProcessDbModel() { ProcessType = processType, ProcessId = processId, DueUtc = dueUtc });
				}
				else
				{
					queuedProcess.DueUtc = dueUtc;
				}
				await dbScope.Db.SaveChangesAsync();
			}
		}

		public Task<IEnumerable<QueuedProcess>> GetQueuedProcessesAsync(DateTime dueSinceUtc, int batchSize)
		{
			using (var dbScope = _dbScopeFactory.Create())
			{
				return Task.FromResult(dbScope.Db.GetQueuedProcesses(dueSinceUtc, batchSize).Select(x => new QueuedProcess(x.ProcessType, x.ProcessId, x.DueUtc)));
			}
		}

		public async Task RemoveQueuedProcessAsync(string processType, string processId)
		{
			using (var dbScope = _dbScopeFactory.Create())
			{
				var queuedProcess = dbScope.Db.GetQueuedProcess(processType, processId);
				if (queuedProcess != null)
				{
					dbScope.Db.QueuedProcess.Remove(queuedProcess);
					await dbScope.Db.SaveChangesAsync();
				}
			}
		}

		public class ProcessManagerStateDbContext : DbContext
		{
			public DbSet<QueuedProcessDbModel> QueuedProcess { get; set; }

			public ProcessManagerStateDbContext(DbContextOptions<ProcessManagerStateDbContext> options) : base(options)
			{
			}

			protected override void OnModelCreating(ModelBuilder builder)
			{
				builder.ApplyConfiguration(new QueuedProcessDbModelConfig());
			}

			public QueuedProcessDbModel GetQueuedProcess(string processType, string processId) => this.QueuedProcess.Find(new { processType, processId });
			public IEnumerable<QueuedProcessDbModel> GetQueuedProcesses(DateTime sinceUtc, int top) => GetProcessesInternal(this, sinceUtc, top);

			private static Func<ProcessManagerStateDbContext, DateTime, int, IEnumerable<QueuedProcessDbModel>> GetProcessesInternal =
				EF.CompileQuery((ProcessManagerStateDbContext context, DateTime sinceUtc, int top) =>
					context.QueuedProcess.Where(x => x.DueUtc >= sinceUtc).OrderByDescending(x => x.DueUtc).Take(top));
		}

		public class QueuedProcessDbModel
		{
			public string ProcessType { get; set; }
			public string ProcessId { get; set; }
			public DateTime DueUtc { get; set; }
		}

		public class QueuedProcessDbModelConfig : IEntityTypeConfiguration<QueuedProcessDbModel>
		{
			public void Configure(EntityTypeBuilder<QueuedProcessDbModel> builder)
			{
				builder.HasKey(x => new { x.ProcessType, x.ProcessId });
			}
		}
	}
}
