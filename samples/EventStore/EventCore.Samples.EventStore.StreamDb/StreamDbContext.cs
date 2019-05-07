using EventCore.Samples.EventStore.StreamDb.Config;
using EventCore.Samples.EventStore.StreamDb.DbModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventCore.Samples.EventStore.StreamDb
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

		public long? GetLastStreamPositionByStreamId(string streamId) => GetLastStreamPositionByStreamIdStatic(this, streamId);
		public IEnumerable<StreamEventDbModel> GetStreamEventsByStreamId(string streamId, long minStreamPosition) => GetStreamEventsByStreamIdStatic(this, streamId, minStreamPosition);
		public IEnumerable<StreamEventDbModel> GetStreamEventsByGlobalPosition(string streamId, long minGlobalPosition) => GetStreamEventsByGlobalPositionStatic(this, streamId, minGlobalPosition);
		public IEnumerable<StreamEventDbModel> GetAllStreamEventsByGlobalPosition(long minGlobalPosition) => GetAllStreamEventsByGlobalPositionStatic(this, minGlobalPosition);
		public IEnumerable<StreamEventDbModel> GetSubscriptionEventsByGlobalPosition(string subscriptionName, long minGlobalPosition) => GetSubscriptionEventsByGlobalPositionStatic(this, subscriptionName, minGlobalPosition);

		// Note: Avoiding use of async queries due to this bug/performance issue that may no longer by an issue.
		// Tldr - async is an order of magnitude slower with tables that have varbinary(max).
		// https://stackoverflow.com/questions/28543293/entity-framework-async-operation-takes-ten-times-as-long-to-complete

		private static Func<StreamDbContext, string, long?> GetLastStreamPositionByStreamIdStatic =
			EF.CompileQuery((StreamDbContext context, string streamId) =>
				context.StreamEvent
					.Where(x => x.StreamId == streamId) // No string transformation (i.e. ToUpper) assuming db is case insensitive by default.
					.Max(x => (long?)x.StreamPosition)
			);

		private static Func<StreamDbContext, string, long, IOrderedQueryable<StreamEventDbModel>> GetStreamEventsByStreamIdStatic =
			EF.CompileQuery((StreamDbContext context, string streamId, long minStreamPosition) =>
				context.StreamEvent
					.Where(x => x.StreamId == streamId && x.StreamPosition >= minStreamPosition) // No string transformation (i.e. ToUpper) assuming db is case insensitive by default.
					.OrderBy(x => x.StreamPosition)
			);

		private static Func<StreamDbContext, string, long, IEnumerable<StreamEventDbModel>> GetStreamEventsByGlobalPositionStatic =
			EF.CompileQuery((StreamDbContext context, string streamId, long minGlobalPosition) =>
				context.StreamEvent
					.Where(x => x.StreamId == streamId && x.GlobalPosition >= minGlobalPosition)
					.OrderBy(x => x.StreamPosition)
			);

		private static Func<StreamDbContext, long, IEnumerable<StreamEventDbModel>> GetAllStreamEventsByGlobalPositionStatic =
			EF.CompileQuery((StreamDbContext context, long minGlobalPosition) =>
				context.StreamEvent
					.Where(x => x.GlobalPosition >= minGlobalPosition)
					.OrderBy(x => x.StreamPosition)
			);

		private static Func<StreamDbContext, string, long, IEnumerable<StreamEventDbModel>> GetSubscriptionEventsByGlobalPositionStatic =
			EF.CompileQuery((StreamDbContext context, string subscriptionName, long minGlobalPosition) =>
				from se in context.StreamEvent
				from sub in context.SubscriptionFilter.Where(x => x.SubscriptionName == subscriptionName && se.StreamId.StartsWith(x.StreamIdPrefix))
				orderby se.GlobalPosition
				select se
			);
	}
}
