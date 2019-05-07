using EventCore.Samples.EventStore.StreamDb.Config;
using EventCore.Samples.EventStore.StreamDb.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EventCore.Samples.EventStore.StreamDb
{
	public class StreamDbContext : DbContext
	{
		public DbSet<StreamEventDbModel> StreamEvent { get; set; }

		public StreamDbContext(DbContextOptions<StreamDbContext> options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.ApplyConfiguration(new StreamEventDbModelConfig());
		}

		public Task<long?> GetMaxEventNumberAsync(string streamId) => GetMaxEventNumberAsyncStatic(this, streamId);
		public Task<IOrderedQueryable<StreamEventDbModel>> GetStreamEventsByStreamAsync(string streamId, long minEventNumber) => GetStreamEventsByStreamAsyncStatic(this, streamId, minEventNumber);
		public AsyncEnumerable<string> GetStreamIdsByMinGlobalIdAsync(string streamId, long minGlobalId) => GetStreamIdsByMinGlobalIdAsyncStatic(this, streamId, minGlobalId);

		private static Func<StreamDbContext, string, Task<long?>> GetMaxEventNumberAsyncStatic =
			EF.CompileAsyncQuery((StreamDbContext context, string streamId) =>
				context.StreamEvent
					.Where(x => x.StreamId == streamId) // No string transformation (i.e. ToUpper) assuming db is case insensitive by default.
					.Max(x => (long?)x.EventNumber)
			);

		private static Func<StreamDbContext, string, long, Task<IOrderedQueryable<StreamEventDbModel>>> GetStreamEventsByStreamAsyncStatic =
			EF.CompileAsyncQuery((StreamDbContext context, string streamId, long fromEventNumber) =>
				context.StreamEvent
					.Where(x => x.StreamId == streamId && x.EventNumber >= fromEventNumber) // No string transformation (i.e. ToUpper) assuming db is case insensitive by default.
					.OrderBy(x => x.EventNumber)
			);
		
		private static Func<StreamDbContext, string, long, AsyncEnumerable<string>> GetStreamIdsByMinGlobalIdAsyncStatic =
			EF.CompileAsyncQuery((StreamDbContext context, string streamId, long minGlobalId) =>
				context.StreamEvent
					.Where(x => x.GlobalId >= minGlobalId)
					.Select(x => x.StreamId.ToUpper()) // Must upper for distinct to return case insensitive.
					.Distinct()
			);
	}
}
