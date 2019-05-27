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
	public class ProcessExecutionQueue : IProcessExecutionQueue
	{
		private readonly IDbContextScopeFactory<ProcessExecutionQueueDbContext> _dbScopeFactory;

		public ProcessExecutionQueue(IDbContextScopeFactory<ProcessExecutionQueueDbContext> dbScopeFactory)
		{
			_dbScopeFactory = dbScopeFactory;
		}

		public async Task CompleteExecutionAsync(string executionId, string errorMessage)
		{
			using (var dbScope = _dbScopeFactory.Create())
			{
				var execution = await dbScope.Db.GetProcessExecutionAsync(executionId);
				if (execution == null || execution.IsComplete)
				{
					return;
				}
				else
				{
					execution.IsComplete = true;
					execution.ErrorMessage = errorMessage;
				}
			}
		}

		public async Task EnqueueExecutionAsync(string executionId, ProcessIdentifier processId, DateTime executeAfterUtc)
		{
			using (var dbScope = _dbScopeFactory.Create())
			{
				dbScope.Db.ProcessExecution.Add(new ProcessExecutionDbModel() { ExecutionId = executionId, ProcessType = processId.ProcessType, CorrelationId = processId.CorrelationId, ExecuteAfterUtc = executeAfterUtc });
				await dbScope.Db.SaveChangesAsync();
			}
		}

		public async Task<ExecutionQueueItem> GetNextExecutionAsync(DateTime maxExecuteAfterUtc, IEnumerable<ProcessIdentifier> excludeProcessIds)
		{
			using (var dbScope = _dbScopeFactory.Create())
			{
				var execution = await dbScope.Db.GetNextProcessExecutionAsync(maxExecuteAfterUtc, excludeProcessIds);
				if (execution == null)
				{
					return null;
				}
				else
				{
					return new ExecutionQueueItem(execution.ExecutionId, new ProcessIdentifier(execution.ProcessType, execution.CorrelationId), execution.ExecuteAfterUtc);
				}
			}
		}

		public class ProcessExecutionQueueDbContext : DbContext
		{
			public DbSet<ProcessExecutionDbModel> ProcessExecution { get; set; }

			public ProcessExecutionQueueDbContext(DbContextOptions<ProcessExecutionQueueDbContext> options) : base(options)
			{
			}

			protected override void OnModelCreating(ModelBuilder builder)
			{
				builder.ApplyConfiguration(new ProcessExecutionDbModelConfig());
			}

			public Task<ProcessExecutionDbModel> GetProcessExecutionAsync(string executionId) => this.ProcessExecution.FindAsync(executionId);
			public Task<ProcessExecutionDbModel> GetNextProcessExecutionAsync(DateTime maxExecuteAfterUtc, IEnumerable<ProcessIdentifier> excludeProcessIds) => GetNextProcessExecutionAsyncInternal(this, maxExecuteAfterUtc, excludeProcessIds.Select(x => x.ProcessType + x.CorrelationId));

			private static Func<ProcessExecutionQueueDbContext, DateTime, IEnumerable<string>, Task<ProcessExecutionDbModel>> GetNextProcessExecutionAsyncInternal =
				EF.CompileAsyncQuery((ProcessExecutionQueueDbContext context, DateTime maxExecuteAfterUtc, IEnumerable<string> excludeProcessIds) =>
					context.ProcessExecution
						.Where(x => x.ExecuteAfterUtc <= maxExecuteAfterUtc)
						.Where(x => !excludeProcessIds.Contains(x.ProcessType + x.CorrelationId))
						.OrderBy(x => x.ExecuteAfterUtc)
						.FirstOrDefault());
		}

		public class ProcessExecutionDbModel
		{
			public string ExecutionId { get; set; }
			public string ProcessType { get; set; }
			public string CorrelationId { get; set; }
			public DateTime ExecuteAfterUtc { get; set; }
			public bool IsComplete { get; set; } = false;
			public string ErrorMessage { get; set; }
		}

		public class ProcessExecutionDbModelConfig : IEntityTypeConfiguration<ProcessExecutionDbModel>
		{
			public void Configure(EntityTypeBuilder<ProcessExecutionDbModel> builder)
			{
				builder.HasKey(x => x.ExecutionId);
			}
		}
	}
}
