using EventCore.EventSourcing;
using EventCore.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventCore.AggregateRoots.Tests
{
	public class AggregateRootStateBusinessEventResolverTests
	{
		private class TestBusinessEvent1 : BusinessEvent
		{
			public TestBusinessEvent1(BusinessEventMetadata metadata) : base(metadata) { }
		}

		private class TestBusinessEvent2 : BusinessEvent
		{
			public TestBusinessEvent2(BusinessEventMetadata metadata) : base(metadata) { }
		}

		private class TestState : IAggregateRootState,
			IApplyBusinessEvent<TestBusinessEvent1>,
			IApplyBusinessEvent<TestBusinessEvent2>
		{
			public long? StreamPositionCheckpoint => throw new NotImplementedException();
			public Task AddCausalIdToHistoryAsync(string causalId) => throw new NotImplementedException();
			public Task HydrateFromCheckpointAsync(Func<Func<StreamEvent, Task>, Task> streamLoaderAsync, CancellationToken cancellationToken) => throw new NotImplementedException();
			public Task<bool> IsCausalIdInHistoryAsync(string causalId) => throw new NotImplementedException();

			public Task ApplyBusinessEventAsync(string streamId, long position, TestBusinessEvent1 e, CancellationToken cancellationToken) => throw new NotImplementedException();
			public Task ApplyBusinessEventAsync(string streamId, long position, TestBusinessEvent2 e, CancellationToken cancellationToken) => throw new NotImplementedException();
		}

		[Fact]
		public void construct_with_applied_business_event_types()
		{
			var resolver = new AggregateRootStateBusinessEventResolver<TestState>(NullStandardLogger.Instance);
			var e1 = new TestBusinessEvent1(BusinessEventMetadata.Empty);
			var e2 = new TestBusinessEvent1(BusinessEventMetadata.Empty);
			Assert.True(resolver.CanUnresolve(e1));
			Assert.True(resolver.CanUnresolve(e2));
		}
	}
}
