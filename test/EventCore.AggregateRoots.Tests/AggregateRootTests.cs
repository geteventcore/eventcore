using EventCore.EventSourcing;
using EventCore.Utilities;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EventCore.AggregateRoots.Tests
{
	public class AggregateRootTests
	{
		private class TestBusinessEvent : BusinessEvent
		{
			public TestBusinessEvent(BusinessEventMetadata metadata) : base(metadata) { }
		}

		[Fact]
		public void handle_command_should_return_semantic_validation_errors()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public void handle_command_should_return_duplicate_command_id_validation_errors()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public void handle_command_should_create_state_from_serialized_data()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public void handle_command_should_return_state_validation_errors()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public void handle_command_should_process_command_to_committed_events_and_return_success()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public void handle_command_should_save_serialized_state()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public async Task try_load_serialized_state_should_return_null_when_not_support_serialization()
		{
			// Will throw null reference if attempts to load state.
			await AggregateRoot<IAggregateRootState>.TryLoadSerializeStateAsync(false, null, null, null, null);
		}

		[Fact]
		public async Task try_load_serialized_state_should_return_null_when_exception()
		{
			var mockRepo = new Mock<ISerializedAggregateRootStateRepo>();
			mockRepo.Setup(x => x.LoadStateAsync(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception());

			// Will throw null reference if attempts to save state.
			await AggregateRoot<IAggregateRootState>.TryLoadSerializeStateAsync(false, null, null, mockRepo.Object, NullStandardLogger.Instance);
		}

		[Fact]
		public void try_load_serialized_state_should_return_loaded_state()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public async Task try_save_serialized_state_should_do_nothing_null_when_agg_root_not_support_serialization()
		{
			var mockState = new Mock<IAggregateRootState>();
			mockState.Setup(x => x.SupportsSerialization).Returns(true);

			// Will throw null reference if attempts to save state.
			await AggregateRoot<IAggregateRootState>.TrySaveSerializeStateAsync(mockState.Object, false, null, null, null, null);
		}

		[Fact]
		public async Task try_save_serialized_state_should_do_nothing_null_when_state_not_support_serialization()
		{
			var mockState = new Mock<IAggregateRootState>();
			mockState.Setup(x => x.SupportsSerialization).Returns(false);

			// Will throw null reference if attempts to save state.
			await AggregateRoot<IAggregateRootState>.TrySaveSerializeStateAsync(mockState.Object, true, null, null, null, null);
		}

		[Fact]
		public async Task try_save_serialized_state_should_do_nothing_when_exception()
		{
			var mockState = new Mock<IAggregateRootState>();
			mockState.Setup(x => x.SupportsSerialization).Returns(true);
			mockState.Setup(x => x.SerializeAsync()).ThrowsAsync(new Exception());

			// Will throw null reference if attempts to save state.
			await AggregateRoot<IAggregateRootState>.TrySaveSerializeStateAsync(mockState.Object, true, null, null, null, NullStandardLogger.Instance);
		}

		[Fact]
		public async Task try_save_serialized_state_should_save_given_state()
		{
			var aggregateRootName = "ar";
			var aggregateRootId = "1";
			var serializedState = "{}";
			var mockState = new Mock<IAggregateRootState>();
			var mockRepo = new Mock<ISerializedAggregateRootStateRepo>();

			mockState.Setup(x => x.SupportsSerialization).Returns(true);
			mockState.Setup(x => x.SerializeAsync()).ReturnsAsync(serializedState);
			mockRepo.Setup(x => x.SaveStateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

			await AggregateRoot<IAggregateRootState>.TrySaveSerializeStateAsync(mockState.Object, true, aggregateRootName, aggregateRootId, mockRepo.Object, NullStandardLogger.Instance);

			mockRepo.Verify(x => x.SaveStateAsync(aggregateRootName, aggregateRootId, serializedState));
		}

		[Fact]
		public async Task process_events_result_should_do_nothing_with_empty_events()
		{
			var mockEventsResult = new Mock<ICommandEventsResult>();
			mockEventsResult.Setup(x => x.Events).Returns(ImmutableList<BusinessEvent>.Empty);

			// Will throw null reference if attempts to process events.
			await AggregateRoot<IAggregateRootState>.ProcessEventsResultAsync(mockEventsResult.Object, null, null, null, null, null);
		}

		[Fact]
		public async Task process_events_result_should_throw_when_can_not_unresolve_event()
		{
			var e = new TestBusinessEvent(BusinessEventMetadata.Empty);
			var events = new List<BusinessEvent>() { e }.ToImmutableList();
			var mockEventsResult = new Mock<ICommandEventsResult>();
			var mockResolver = new Mock<IBusinessEventResolver>();

			mockEventsResult.Setup(x => x.Events).Returns(events);
			mockResolver.Setup(x => x.CanUnresolve(e)).Returns(false);

			await Assert.ThrowsAsync<InvalidOperationException>(() => AggregateRoot<IAggregateRootState>.ProcessEventsResultAsync(mockEventsResult.Object, null, null, null, mockResolver.Object, null));
		}

		[Fact]
		public async Task process_events_result_should_throw_when_concurrency_conflict()
		{
			var regionId = "x";
			var streamId = "s";
			var streamPositionCheckpoint = 0;
			var e = new TestBusinessEvent(BusinessEventMetadata.Empty);
			var events = new List<BusinessEvent>() { e }.ToImmutableList();
			var eventType = typeof(TestBusinessEvent).Name;
			var dataText = "{}";
			var dataBytes = Encoding.Unicode.GetBytes(dataText);
			var unresolvedEvent = new UnresolvedBusinessEvent(eventType, dataBytes);

			var mockEventsResult = new Mock<ICommandEventsResult>();
			var mockResolver = new Mock<IBusinessEventResolver>();
			var mockState = new Mock<IAggregateRootState>();
			var mockStreamClient = new Mock<IStreamClient>();

			mockEventsResult.Setup(x => x.Events).Returns(events);
			mockResolver.Setup(x => x.CanUnresolve(e)).Returns(true);
			mockResolver.Setup(x => x.UnresolveEvent(e)).Returns(unresolvedEvent);
			mockState.Setup(x => x.StreamPositionCheckpoint).Returns(streamPositionCheckpoint);
			mockStreamClient.Setup(x => x.CommitEventsToStreamAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<IEnumerable<CommitEvent>>())).ReturnsAsync(CommitResult.ConcurrencyConflict);

			await Assert.ThrowsAsync<InvalidOperationException>(() => AggregateRoot<IAggregateRootState>.ProcessEventsResultAsync(mockEventsResult.Object, regionId, streamId, mockState.Object, mockResolver.Object, mockStreamClient.Object));
		}

		[Fact]
		public async Task process_events_result_should_commit_events()
		{
			var regionId = "x";
			var streamId = "s";
			var streamPositionCheckpoint = 0;
			var e = new TestBusinessEvent(BusinessEventMetadata.Empty);
			var events = new List<BusinessEvent>() { e }.ToImmutableList();
			var eventType = typeof(TestBusinessEvent).Name;
			var dataText = "{}";
			var dataBytes = Encoding.Unicode.GetBytes(dataText);
			var unresolvedEvent = new UnresolvedBusinessEvent(eventType, dataBytes);

			var mockEventsResult = new Mock<ICommandEventsResult>();
			var mockResolver = new Mock<IBusinessEventResolver>();
			var mockState = new Mock<IAggregateRootState>();
			var mockStreamClient = new Mock<IStreamClient>();

			mockEventsResult.Setup(x => x.Events).Returns(events);
			mockResolver.Setup(x => x.CanUnresolve(e)).Returns(true);
			mockResolver.Setup(x => x.UnresolveEvent(e)).Returns(unresolvedEvent);
			mockState.Setup(x => x.StreamPositionCheckpoint).Returns(streamPositionCheckpoint);
			mockStreamClient.Setup(x => x.CommitEventsToStreamAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<IEnumerable<CommitEvent>>())).ReturnsAsync(CommitResult.Success);

			await AggregateRoot<IAggregateRootState>.ProcessEventsResultAsync(mockEventsResult.Object, regionId, streamId, mockState.Object, mockResolver.Object, mockStreamClient.Object);

			mockStreamClient.Verify(x =>
				x.CommitEventsToStreamAsync(
					regionId, streamId, streamPositionCheckpoint,
					It.Is<IEnumerable<CommitEvent>>(ces => ces.FirstOrDefault(ce => ce.EventType == eventType && Encoding.Unicode.GetString(ce.Data) == dataText) != null)
					)
				);
		}
	}
}
