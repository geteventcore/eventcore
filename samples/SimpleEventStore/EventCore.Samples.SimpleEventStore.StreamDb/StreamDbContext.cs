using EventCore.Samples.SimpleEventStore.StreamDb.Config;
using EventCore.Samples.SimpleEventStore.StreamDb.DbModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventCore.Samples.SimpleEventStore.StreamDb
{
	public class StreamDbContext : DbContext
	{
		public DbSet<SubscriptionFilterDbModel> SubscriptionFilter { get; set; }
		public DbSet<StreamEventDbModel> StreamEvent { get; set; }

		public StreamDbContext(DbContextOptions<StreamDbContext> options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.ApplyConfiguration(new SubscriptionFilterDbModelConfig());
			builder.ApplyConfiguration(new StreamEventDbModelConfig());
		}

		public long? GetMaxGlobalPosition() => GetMaxGlobalPositionInternal(this);
		public long? GetMaxStreamPositionByStreamId(string streamId) => GetMaxStreamPositionByStreamIdInternal(this, streamId);
		// public IEnumerable<StreamEventDbModel> GetStreamEventsByStreamId(string streamId, long minStreamPosition) => GetStreamEventsByStreamIdInternal(this, streamId, minStreamPosition);
		// public IEnumerable<StreamEventDbModel> GetStreamEventsByGlobalPosition(string streamId, long minGlobalPosition) => GetStreamEventsByGlobalPositionInternal(this, streamId, minGlobalPosition);
		// public IEnumerable<StreamEventDbModel> GetAllStreamEventsByGlobalPosition(long minGlobalPosition) => GetAllStreamEventsByGlobalPositionInternal(this, minGlobalPosition);
		// public IEnumerable<StreamEventDbModel> GetSubscriptionEventsByGlobalPosition(string subscriptionName, long minGlobalPosition) => GetSubscriptionEventsByGlobalPositionInternal(this, subscriptionName, minGlobalPosition);

		// Note: Avoiding use of async queries due to this bug/performance issue that may no longer by an issue.
		// Tldr - async is an order of magnitude slower with tables that have varbinary(max), or was at the time of this discussion.
		// https://stackoverflow.com/questions/28543293/entity-framework-async-operation-takes-ten-times-as-long-to-complete

		private static Func<StreamDbContext, long?> GetMaxGlobalPositionInternal =
			EF.CompileQuery((StreamDbContext context) => context.StreamEvent.Max(x => (long?)x.GlobalPosition));

		private static Func<StreamDbContext, string, long?> GetMaxStreamPositionByStreamIdInternal =
			EF.CompileQuery((StreamDbContext context, string streamId) =>
				context.StreamEvent
					.Where(x => x.StreamId == streamId) // No string transformation (i.e. ToUpper) assuming db is case insensitive by default.
					.Max(x => (long?)x.StreamPosition)
			);

		private static Func<StreamDbContext, string, long, IEnumerable<StreamEventDbModel>> GetStreamEventsByStreamIdInternal =
			EF.CompileQuery((StreamDbContext context, string streamId, long minStreamPosition) =>
				context.StreamEvent
					.Where(x => x.StreamId == streamId && x.StreamPosition >= minStreamPosition) // No string transformation (i.e. ToUpper) assuming db is case insensitive by default.
					.OrderBy(x => x.StreamPosition)
			);

		private static Func<StreamDbContext, string, long, IEnumerable<StreamEventDbModel>> GetStreamEventsByGlobalPositionInternal =
			EF.CompileQuery((StreamDbContext context, string streamId, long minGlobalPosition) =>
				context.StreamEvent
					.Where(x => x.StreamId == streamId && x.GlobalPosition >= minGlobalPosition)
					.OrderBy(x => x.StreamPosition)
			);

		private static Func<StreamDbContext, long, IEnumerable<StreamEventDbModel>> GetAllStreamEventsByGlobalPositionInternal =
			EF.CompileQuery((StreamDbContext context, long minGlobalPosition) =>
				context.StreamEvent
					.Where(x => x.GlobalPosition >= minGlobalPosition)
					.OrderBy(x => x.StreamPosition)
			);
		
		private static Func<StreamDbContext, string, long, IEnumerable<StreamEventDbModel>> GetSubscriptionEventsByGlobalPositionInternal =
			EF.CompileQuery((StreamDbContext context, string subscriptionName, long minGlobalPosition) =>
				context.SubscriptionFilter
					.Where(x => x.SubscriptionName == subscriptionName)
					.SelectMany(sub => context.StreamEvent.Where(se => se.StreamId.ToUpper().StartsWith(sub.StreamIdPrefix.ToUpper())))
					.Distinct()
					.OrderBy(x => x.GlobalPosition)
			);
	}
}
