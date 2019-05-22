using EventCore.ProcessManagers;
using EventCore.Samples.Ecommerce.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.ProcessManagers.EmailManagement
{
	public class ProcessManagerStateRepo : IProcessManagerStateRepo
	{
		private readonly IDbContextScopeFactory<ProcessManagerStateDbContext> _dbScopeFactory;

		public ProcessManagerStateRepo(IDbContextScopeFactory<ProcessManagerStateDbContext> dbScopeFactory)
		{
			_dbScopeFactory = dbScopeFactory;
		}

		public async Task AddOrUpdateQueuedProcessAsync(string processType, string processId, DateTime? dueUtc, DateTime defaultDueUtc, int executionAttempts)
		{
			using (var dbScope = _dbScopeFactory.Create())
			{
				var queuedProcess = await dbScope.Db.GetQueuedProcessAsync(processType, processId);
				if (queuedProcess == null)
				{
					dbScope.Db.QueuedProcess.Add(new QueuedProcessDbModel() { ProcessType = processType, ProcessId = processId, DueUtc = dueUtc.GetValueOrDefault(defaultDueUtc), ExecutionAttempts = executionAttempts });
				}
				else
				{
					queuedProcess.DueUtc = dueUtc.GetValueOrDefault(queuedProcess.DueUtc);
					queuedProcess.ExecutionAttempts = executionAttempts;
				}
				await dbScope.Db.SaveChangesAsync();
			}
		}

		public async Task<QueuedProcess> GetNextQueuedProcessAsync(DateTime dueBeforeUtc, int maxAttempts)
		{
			using (var dbScope = _dbScopeFactory.Create())
			{
				var record = await dbScope.Db.GetNextQueuedProcessAsync(dueBeforeUtc, maxAttempts);
				if (record == null)
				{
					return null;
				}
				else
				{
					return new QueuedProcess(record.ProcessType, record.ProcessId, record.DueUtc);
				}
			}
		}

		public async Task RemoveQueuedProcessAsync(string processType, string processId)
		{
			using (var dbScope = _dbScopeFactory.Create())
			{
				var queuedProcess = await dbScope.Db.GetQueuedProcessAsync(processType, processId);
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

			public Task<QueuedProcessDbModel> GetQueuedProcessAsync(string processType, string processId) => this.QueuedProcess.FindAsync(new { processType, processId });
			public Task<QueuedProcessDbModel> GetNextQueuedProcessAsync(DateTime dueBeforeUtc, int maxAttempts) => GetNextQueuedProcessAsyncInternal(this, dueBeforeUtc, maxAttempts);

			private static Func<ProcessManagerStateDbContext, DateTime, int, Task<QueuedProcessDbModel>> GetNextQueuedProcessAsyncInternal =
				EF.CompileAsyncQuery((ProcessManagerStateDbContext context, DateTime dueBeforeUtc, int maxAttempts) =>
					context.QueuedProcess.Where(x => x.DueUtc < dueBeforeUtc && x.ExecutionAttempts < maxAttempts).OrderBy(x => x.DueUtc).FirstOrDefault());
		}

		public class QueuedProcessDbModel
		{
			public string ProcessType { get; set; }
			public string ProcessId { get; set; }
			public DateTime DueUtc { get; set; }
			public int ExecutionAttempts { get; set; }
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
