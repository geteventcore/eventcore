using EventCore.EventSourcing;
using EventCore.Utilities;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventCore.AggregateRoots.Tests
{
	public class AggregateRootTests
	{
		private class TestException : Exception { }

		private class TestBusinessEvent : BusinessEvent
		{
			public TestBusinessEvent(BusinessEventMetadata metadata) : base(metadata) { }
		}

		private class TestAggregateRoot : AggregateRoot<IAggregateRootState>
		{
			private readonly bool _supportsSerializedState;
			public TestAggregateRoot(AggregateRootDependencies<IAggregateRootState> dependencies, string context, string aggregateRootName) : base(dependencies, context, aggregateRootName)
			{
			}
		}

		[Fact]
		public async Task handle_command_should_rethrow()
		{
			var mockCommand = new Mock<ICommand>();
			var dependencies = new AggregateRootDependencies<IAggregateRootState>(NullStandardLogger.Instance, null, null, null, null, null);
			var ar = new TestAggregateRoot(dependencies, null, null);

			mockCommand.Setup(x => x.ValidateSemantics()).Throws(new TestException());

			await Assert.ThrowsAsync<TestException>(() => ar.HandleGenericCommandAsync(mockCommand.Object, CancellationToken.None));
		}

		[Fact]
		public async Task handle_command_should_return_semantic_validation_errors()
		{
			var error = "validation error";
			var errors = new List<string>() { error }.ToImmutableList();
			var mockCommand = new Mock<ICommand>();
			var mockCommandValidationResult = new Mock<ICommandValidationResult>();
			var dependencies = new AggregateRootDependencies<IAggregateRootState>(NullStandardLogger.Instance, null, null, null, null, null);
			var ar = new TestAggregateRoot(dependencies, null, null);

			mockCommandValidationResult.Setup(x => x.IsValid).Returns(false);
			mockCommandValidationResult.Setup(x => x.Errors).Returns(errors);
			mockCommand.Setup(x => x.ValidateSemantics()).Returns(mockCommandValidationResult.Object);

			var result = await ar.HandleGenericCommandAsync(mockCommand.Object, CancellationToken.None);

			Assert.False(result.IsSuccess);
			Assert.Contains(error, result.ValidationErrors);
		}

		[Fact]
		public async Task handle_command_should_build_correct_stream_id()
		{
			var regionId = "x";
			var context = "ctx";
			var aggregateRootName = "ar";
			var aggregateRootId = "1";

			var mockCommand = new Mock<ICommand>();
			var mockCommandValidationResult = new Mock<ICommandValidationResult>();
			var mockStreamIdBuilder = new Mock<IStreamIdBuilder>();

			var dependencies = new AggregateRootDependencies<IAggregateRootState>(NullStandardLogger.Instance, null, mockStreamIdBuilder.Object, null, null, null);
			var ar = new TestAggregateRoot(dependencies, context, aggregateRootName);

			mockCommandValidationResult.Setup(x => x.IsValid).Returns(true); ;
			mockCommand.Setup(x => x.ValidateSemantics()).Returns(mockCommandValidationResult.Object);
			mockCommand.Setup(x => x.GetRegionId()).Returns(regionId);
			mockCommand.Setup(x => x.GetAggregateRootId()).Returns(aggregateRootId);

			try
			{
				await ar.HandleGenericCommandAsync(mockCommand.Object, CancellationToken.None);
			}
			catch (Exception)
			{
				// Ignore exception.
			}

			mockStreamIdBuilder.Verify(x => x.Build(regionId, context, aggregateRootName, aggregateRootId));
		}

		[Fact]
		public async Task handle_command_should_create_state()
		{
			var regionId = "x";
			var context = "ctx";
			var aggregateRootName = "ar";
			var aggregateRootId = "1";
			var streamId = "sId";

			var mockCommand = new Mock<ICommand>();
			var mockCommandValidationResult = new Mock<ICommandValidationResult>();
			var mockStreamIdBuilder = new Mock<IStreamIdBuilder>();
			var mockStateFactory = new Mock<IAggregateRootStateFactory<IAggregateRootState>>();
			var cancelSource = new CancellationTokenSource();

			var dependencies = new AggregateRootDependencies<IAggregateRootState>(NullStandardLogger.Instance, mockStateFactory.Object, mockStreamIdBuilder.Object, null, null, null);
			var ar = new TestAggregateRoot(dependencies, context, aggregateRootName); // Supports serialized state.

			mockCommandValidationResult.Setup(x => x.IsValid).Returns(true); ;
			mockCommand.Setup(x => x.ValidateSemantics()).Returns(mockCommandValidationResult.Object);
			mockCommand.Setup(x => x.GetRegionId()).Returns(regionId);
			mockCommand.Setup(x => x.GetAggregateRootId()).Returns(aggregateRootId);
			mockStreamIdBuilder.Setup(x => x.Build(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(streamId);

			try
			{
				await ar.HandleGenericCommandAsync(mockCommand.Object, cancelSource.Token);
			}
			catch (Exception)
			{
				// Ignore exception.
			}

			mockStateFactory.Verify(x => x.CreateAndLoadToCheckpointAsync(regionId, context, aggregateRootName, aggregateRootId, cancelSource.Token));
		}

		[Fact]
		public async Task handle_command_should_hydrate_state()
		{
			var aggregateRootName = "ar";
			var streamId = "sId";

			var mockCommand = new Mock<ICommand>();
			var mockCommandValidationResult = new Mock<ICommandValidationResult>();
			var mockStreamIdBuilder = new Mock<IStreamIdBuilder>();
			var mockStateFactory = new Mock<IAggregateRootStateFactory<IAggregateRootState>>();
			var mockState = new Mock<IAggregateRootState>();

			var dependencies = new AggregateRootDependencies<IAggregateRootState>(NullStandardLogger.Instance, mockStateFactory.Object, mockStreamIdBuilder.Object, null, null, null);
			var ar = new TestAggregateRoot(dependencies, null, aggregateRootName);

			mockCommandValidationResult.Setup(x => x.IsValid).Returns(true); ;
			mockCommand.Setup(x => x.ValidateSemantics()).Returns(mockCommandValidationResult.Object);
			mockCommand.Setup(x => x.GetRegionId()).Returns((string)null);
			mockCommand.Setup(x => x.GetAggregateRootId()).Returns((string)null);
			mockStreamIdBuilder.Setup(x => x.Build(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(streamId);
			mockStateFactory.Setup(x => x.CreateAndLoadToCheckpointAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockState.Object);

			try
			{
				await ar.HandleGenericCommandAsync(mockCommand.Object);
			}
			catch (Exception)
			{
				// Ignore exception.
			}

			mockState.Verify(x => x.HydrateAsync(mockStreamClient.Object, streamId));
		}

		[Fact]
		public async Task handle_command_should_return_duplicate_command_id_validation_errors()
		{
			var aggregateRootName = "ar";
			var streamId = "sId";
			var commandId = "cId";

			var mockCommand = new Mock<ICommand>();
			var mockCommandMetadata = new Mock<ICommandMetadata>();
			var mockCommandValidationResult = new Mock<ICommandValidationResult>();
			var mockStreamIdBuilder = new Mock<IStreamIdBuilder>();
			var mockStateFactory = new Mock<IAggregateRootStateFactory<IAggregateRootState>>();
			var mockState = new Mock<IAggregateRootState>();

			var dependencies = new AggregateRootDependencies<IAggregateRootState>(NullStandardLogger.Instance, mockStateFactory.Object, mockStreamIdBuilder.Object, null, null, null, null);
			var ar = new TestAggregateRoot(dependencies, null, aggregateRootName);

			mockCommandValidationResult.Setup(x => x.IsValid).Returns(true);
			mockCommandMetadata.Setup(x => x.CommandId).Returns(commandId);
			mockCommand.Setup(x => x.ValidateSemantics()).Returns(mockCommandValidationResult.Object);
			mockCommand.Setup(x => x.GetRegionId()).Returns((string)null);
			mockCommand.Setup(x => x.GetAggregateRootId()).Returns((string)null);
			mockCommand.Setup(x => x._Metadata).Returns(mockCommandMetadata.Object);
			mockStreamIdBuilder.Setup(x => x.Build(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(streamId);
			mockStateFactory.Setup(x => x.Create(null)).Returns(mockState.Object);
			mockState.Setup(x => x.HydrateAsync(It.IsAny<IStreamClient>(), It.IsAny<string>())).Returns(Task.CompletedTask);
			mockState.Setup(x => x.IsCausalIdInRecentHistory(commandId)).Returns(true);

			var result = await ar.HandleGenericCommandAsync(mockCommand.Object);

			Assert.False(result.IsSuccess);
			Assert.Contains("Duplicate command id.", result.ValidationErrors);
		}

		[Fact]
		public async Task handle_command_should_return_state_validation_errors()
		{
			var aggregateRootName = "ar";
			var streamId = "sId";
			var commandId = "cId";
			var error = "state error";
			var errors = new List<string>() { error }.ToImmutableList();

			var mockCommand = new Mock<ICommand>();
			var mockCommandMetadata = new Mock<ICommandMetadata>();
			var mockCommandValidationResultForSemantics = new Mock<ICommandValidationResult>();
			var mockCommandValidationResultForState = new Mock<ICommandValidationResult>();
			var mockStreamIdBuilder = new Mock<IStreamIdBuilder>();
			var mockStateFactory = new Mock<IAggregateRootStateFactory<IAggregateRootState>>();
			var mockStateRepo = new Mock<ISerializedAggregateRootStateRepo>();
			var mockState = new Mock<IAggregateRootState>();
			var mockHandlerFactory = new Mock<ICommandHandlerFactory<IAggregateRootState>>();
			var mockHandler = new Mock<ICommandHandler<IAggregateRootState, ICommand>>();

			var dependencies = new AggregateRootDependencies<IAggregateRootState>(NullStandardLogger.Instance, mockStateFactory.Object, mockStreamIdBuilder.Object, null, null, mockHandlerFactory.Object, mockStateRepo.Object);
			var ar = new TestAggregateRoot(dependencies, null, aggregateRootName);

			mockCommandValidationResultForSemantics.Setup(x => x.IsValid).Returns(true);
			mockCommandMetadata.Setup(x => x.CommandId).Returns(commandId);
			mockCommand.Setup(x => x.ValidateSemantics()).Returns(mockCommandValidationResultForSemantics.Object);
			mockCommand.Setup(x => x.GetRegionId()).Returns((string)null);
			mockCommand.Setup(x => x.GetAggregateRootId()).Returns((string)null);
			mockCommand.Setup(x => x._Metadata).Returns(mockCommandMetadata.Object);
			mockStreamIdBuilder.Setup(x => x.Build(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(streamId);
			mockStateFactory.Setup(x => x.Create(null)).Returns(mockState.Object);
			mockState.Setup(x => x.HydrateAsync(It.IsAny<IStreamClient>(), It.IsAny<string>())).Returns(Task.CompletedTask);
			mockState.Setup(x => x.IsCausalIdInRecentHistory(It.IsAny<string>())).Returns(false);
			mockHandlerFactory.Setup(x => x.Create<ICommand>()).Returns(mockHandler.Object);
			mockCommandValidationResultForState.Setup(x => x.IsValid).Returns(false);
			mockCommandValidationResultForState.Setup(x => x.Errors).Returns(errors);
			mockHandler.Setup(x => x.ValidateForStateAsync(mockState.Object, mockCommand.Object)).ReturnsAsync(mockCommandValidationResultForState.Object);

			var result = await ar.HandleGenericCommandAsync(mockCommand.Object);

			Assert.False(result.IsSuccess);
			Assert.Contains(error, result.ValidationErrors);
		}

		[Fact]
		public async Task handle_command_should_process_command_to_committed_events_and_save_state_and_return_success()
		{
			var regionId = "x";
			var aggregateRootName = "ar";
			var aggregateRootId = "1";
			var streamId = "sId";
			var commandId = "cId";
			var e = new TestBusinessEvent(BusinessEventMetadata.Empty);
			var events = new List<BusinessEvent>() { e }.ToImmutableList();
			var eventType = e.GetType().Name;
			var eventDataText = "{}";
			var eventDataBytes = Encoding.Unicode.GetBytes(eventDataText);
			var unresolvedEvent = new UnresolvedBusinessEvent(eventType, eventDataBytes);
			var streamPositionCheckpoint = 0;
			var commitResult = CommitResult.Success;
			var serializedState = "{}";

			var mockCommand = new Mock<ICommand>();
			var mockCommandMetadata = new Mock<ICommandMetadata>();
			var mockCommandValidationResultForSemantics = new Mock<ICommandValidationResult>();
			var mockCommandValidationResultForState = new Mock<ICommandValidationResult>();
			var mockStreamIdBuilder = new Mock<IStreamIdBuilder>();
			var mockStreamClient = new Mock<IStreamClient>();
			var mockStateFactory = new Mock<IAggregateRootStateFactory<IAggregateRootState>>();
			var mockStateRepo = new Mock<ISerializedAggregateRootStateRepo>();
			var mockState = new Mock<IAggregateRootState>();
			var mockHandlerFactory = new Mock<ICommandHandlerFactory<IAggregateRootState>>();
			var mockHandler = new Mock<ICommandHandler<IAggregateRootState, ICommand>>();
			var mockCommandEventsResult = new Mock<ICommandEventsResult>();
			var mockResolver = new Mock<IBusinessEventResolver>();

			var dependencies = new AggregateRootDependencies<IAggregateRootState>(NullStandardLogger.Instance, mockStateFactory.Object, mockStreamIdBuilder.Object, mockStreamClient.Object, mockResolver.Object, mockHandlerFactory.Object, mockStateRepo.Object);
			var ar = new TestAggregateRoot(dependencies, null, aggregateRootName, true); // Supports serialization.

			mockCommandValidationResultForSemantics.Setup(x => x.IsValid).Returns(true);
			mockCommandMetadata.Setup(x => x.CommandId).Returns(commandId);
			mockCommand.Setup(x => x.ValidateSemantics()).Returns(mockCommandValidationResultForSemantics.Object);
			mockCommand.Setup(x => x.GetRegionId()).Returns(regionId);
			mockCommand.Setup(x => x.GetAggregateRootId()).Returns(aggregateRootId);
			mockCommand.Setup(x => x._Metadata).Returns(mockCommandMetadata.Object);
			mockStreamIdBuilder.Setup(x => x.Build(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(streamId);
			mockStateFactory.Setup(x => x.Create(null)).Returns(mockState.Object);
			mockState.Setup(x => x.HydrateAsync(It.IsAny<IStreamClient>(), It.IsAny<string>())).Returns(Task.CompletedTask);
			mockState.Setup(x => x.IsCausalIdInRecentHistory(It.IsAny<string>())).Returns(false);
			mockState.Setup(x => x.StreamPositionCheckpoint).Returns(streamPositionCheckpoint);
			mockState.Setup(x => x.SupportsSerialization).Returns(true);
			mockState.Setup(x => x.SerializeAsync()).ReturnsAsync(serializedState);
			mockHandlerFactory.Setup(x => x.Create<ICommand>()).Returns(mockHandler.Object);
			mockCommandValidationResultForState.Setup(x => x.IsValid).Returns(true);
			mockHandler.Setup(x => x.ValidateForStateAsync(mockState.Object, mockCommand.Object)).ReturnsAsync(mockCommandValidationResultForState.Object);
			mockHandler.Setup(x => x.ProcessCommandAsync(mockState.Object, mockCommand.Object)).ReturnsAsync(mockCommandEventsResult.Object);
			mockCommandEventsResult.Setup(x => x.Events).Returns(events);
			mockResolver.Setup(x => x.CanUnresolve(e)).Returns(true);
			mockResolver.Setup(x => x.Unresolve(e)).Returns(unresolvedEvent);
			mockStreamClient.Setup(x => x.CommitEventsToStreamAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<IEnumerable<CommitEvent>>())).ReturnsAsync(commitResult);

			var result = await ar.HandleGenericCommandAsync(mockCommand.Object);

			mockStreamClient.Verify(x => x.CommitEventsToStreamAsync(regionId, streamId, streamPositionCheckpoint, It.IsAny<IEnumerable<CommitEvent>>()));
			mockStateRepo.Verify(x => x.SaveStateAsync(aggregateRootName, aggregateRootId, serializedState));
			Assert.True(result.IsSuccess);
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
			mockResolver.Setup(x => x.Unresolve(e)).Returns(unresolvedEvent);
			mockStreamClient.Setup(x => x.CommitEventsToStreamAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<IEnumerable<CommitEvent>>())).ReturnsAsync(CommitResult.ConcurrencyConflict);

			await Assert.ThrowsAsync<InvalidOperationException>(() => AggregateRoot<IAggregateRootState>.ProcessEventsResultAsync(mockEventsResult.Object, null, null, null, mockResolver.Object, mockStreamClient.Object));
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
			var mockStreamClient = new Mock<IStreamClient>();

			mockEventsResult.Setup(x => x.Events).Returns(events);
			mockResolver.Setup(x => x.CanUnresolve(e)).Returns(true);
			mockResolver.Setup(x => x.Unresolve(e)).Returns(unresolvedEvent);
			mockStreamClient.Setup(x => x.CommitEventsToStreamAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<IEnumerable<CommitEvent>>())).ReturnsAsync(CommitResult.Success);

			await AggregateRoot<IAggregateRootState>.ProcessEventsResultAsync(mockEventsResult.Object, regionId, streamId, streamPositionCheckpoint, mockResolver.Object, mockStreamClient.Object);

			mockStreamClient.Verify(x =>
				x.CommitEventsToStreamAsync(
					regionId, streamId, streamPositionCheckpoint,
					It.Is<IEnumerable<CommitEvent>>(ces => ces.FirstOrDefault(ce => ce.EventType == eventType && Encoding.Unicode.GetString(ce.Data) == dataText) != null)
					)
				);
		}
	}
}
