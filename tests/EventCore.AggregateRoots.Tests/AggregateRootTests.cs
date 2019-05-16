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

		private class TestAggregateRoot : AggregateRoot<IAggregateRootState>
		{
			public Func<IAggregateRootState, ICommand, CancellationToken, Task<ICommandResult>> HandleDelegate;

			public TestAggregateRoot(AggregateRootDependencies<IAggregateRootState> dependencies, string context, string aggregateRootName) : base(dependencies, context, aggregateRootName)
			{
			}

			public Task<ICommandResult> HandleCommandAsync(IAggregateRootState s, ICommand c, CancellationToken ct)
			{
				return HandleDelegate(s, c, ct);
			}

			public Task<bool> TestCommitEventsAsync(IImmutableList<IBusinessEvent> events, string regionId, string streamId, long? lastPositionHydrated)
				=> CommitEventsAsync(events, regionId, streamId, lastPositionHydrated);
		}

		[Fact]
		public async Task rethrow_exceptions_when_handle_generic_command()
		{
			var mockCommand = new Mock<ICommand>();
			var dependencies = new AggregateRootDependencies<IAggregateRootState>(NullStandardLogger.Instance, null, null, null, null);
			var ar = new TestAggregateRoot(dependencies, null, null);

			mockCommand.Setup(x => x.ValidateSemantics()).Throws(new TestException());

			await Assert.ThrowsAsync<TestException>(() => ar.HandleGenericCommandAsync(mockCommand.Object, CancellationToken.None));
		}

		[Fact]
		public async Task return_semantic_validation_errors_when_handle_generic_command()
		{
			var error = "validation error";
			var errors = new List<string>() { error }.ToImmutableList();
			var mockCommand = new Mock<ICommand>();
			var mockCommandValidationResult = new Mock<ICommandValidationResult>();
			var dependencies = new AggregateRootDependencies<IAggregateRootState>(NullStandardLogger.Instance, null, null, null, null);
			var ar = new TestAggregateRoot(dependencies, null, null);

			mockCommandValidationResult.Setup(x => x.IsValid).Returns(false);
			mockCommandValidationResult.Setup(x => x.Errors).Returns(errors);
			mockCommand.Setup(x => x.ValidateSemantics()).Returns(mockCommandValidationResult.Object);

			var result = await ar.HandleGenericCommandAsync(mockCommand.Object, CancellationToken.None);

			Assert.False(result.IsSuccess);
			Assert.Contains(error, result.Errors);
		}

		[Fact]
		public async Task build_correct_stream_id_when_handle_generic_command()
		{
			var regionId = "x";
			var context = "ctx";
			var aggregateRootName = "ar";
			var aggregateRootId = "1";

			var mockCommand = new Mock<ICommand>();
			var mockCommandValidationResult = new Mock<ICommandValidationResult>();
			var mockStreamIdBuilder = new Mock<IStreamIdBuilder>();

			var dependencies = new AggregateRootDependencies<IAggregateRootState>(NullStandardLogger.Instance, null, mockStreamIdBuilder.Object, null, null);
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
			var streamId = "sId";
			var mockCommand = new Mock<ICommand>();
			var mockCommandValidationResult = new Mock<ICommandValidationResult>();
			var mockStreamIdBuilder = new Mock<IStreamIdBuilder>();
			var mockStateRepo = new Mock<IAggregateRootStateRepo<IAggregateRootState>>();
			var cancelSource = new CancellationTokenSource();

			var dependencies = new AggregateRootDependencies<IAggregateRootState>(NullStandardLogger.Instance, mockStateRepo.Object, mockStreamIdBuilder.Object, null, null);
			var ar = new TestAggregateRoot(dependencies, null, null);

			mockCommandValidationResult.Setup(x => x.IsValid).Returns(true); ;
			mockCommand.Setup(x => x.ValidateSemantics()).Returns(mockCommandValidationResult.Object);
			mockCommand.Setup(x => x.GetRegionId()).Returns(regionId);
			mockCommand.Setup(x => x.GetAggregateRootId()).Returns(string.Empty);
			mockStreamIdBuilder.Setup(x => x.Build(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(streamId);

			try
			{
				await ar.HandleGenericCommandAsync(mockCommand.Object, cancelSource.Token);
			}
			catch (Exception)
			{
				// Ignore exception.
			}

			mockStateRepo.Verify(x => x.LoadAsync(regionId, streamId, cancelSource.Token));
		}

		[Fact]
		public async Task return_duplicate_command_id_validation_error_when_handle_generic_command()
		{
			var commandId = "cId";
			var mockCommand = new Mock<ICommand>();
			var mockCommandValidationResult = new Mock<ICommandValidationResult>();
			var mockStreamIdBuilder = new Mock<IStreamIdBuilder>();
			var mockStreamClientFactory = new Mock<IStreamClientFactory>();
			var mockStreamClient = new Mock<IStreamClient>();
			var mockStateRepo = new Mock<IAggregateRootStateRepo<IAggregateRootState>>();
			var mockState = new Mock<IAggregateRootState>();
			var cancelSource = new CancellationTokenSource();

			var dependencies = new AggregateRootDependencies<IAggregateRootState>(NullStandardLogger.Instance, mockStateRepo.Object, mockStreamIdBuilder.Object, mockStreamClientFactory.Object, null);
			var ar = new TestAggregateRoot(dependencies, null, null);

			mockCommandValidationResult.Setup(x => x.IsValid).Returns(true);
			mockCommand.Setup(x => x.ValidateSemantics()).Returns(mockCommandValidationResult.Object);
			mockCommand.Setup(x => x.GetRegionId()).Returns(string.Empty);
			mockCommand.Setup(x => x.GetAggregateRootId()).Returns(string.Empty);
			mockCommand.Setup(x => x.GetCommandId()).Returns(commandId);
			mockStreamIdBuilder.Setup(x => x.Build(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(string.Empty);
			mockStreamClientFactory.Setup( x => x.Create(It.IsAny<string>())).Returns(mockStreamClient.Object);
			mockStreamClient.Setup(x => x.FirstPositionInStream).Returns(0);
			mockStateRepo.Setup(x => x.LoadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockState.Object);
			mockState.Setup(x => x.IsCausalIdInHistoryAsync(commandId)).ReturnsAsync(true);

			var result = await ar.HandleGenericCommandAsync(mockCommand.Object, cancelSource.Token);

			Assert.False(result.IsSuccess);
			Assert.Contains("Duplicate command id.", result.Errors);
		}

		[Fact]
		public async Task return_handler_errors_when_handle_generic_command()
		{
			var aggregateRootName = "ar";
			var streamId = "sId";
			var commandId = "cId";
			var error = "state error";
			var errors = new List<string>() { error }.ToImmutableList();

			var mockCommand = new Mock<ICommand>();
			var mockCommandValidationResultForSemantics = new Mock<ICommandValidationResult>();
			var mockCommandResult = new Mock<ICommandResult>();
			var mockStreamIdBuilder = new Mock<IStreamIdBuilder>();
			var mockStreamClientFactory = new Mock<IStreamClientFactory>();
			var mockStreamClient = new Mock<IStreamClient>();
			var mockStateRepo = new Mock<IAggregateRootStateRepo<IAggregateRootState>>();
			var mockState = new Mock<IAggregateRootState>();
			var cancelSource = new CancellationTokenSource();

			var dependencies = new AggregateRootDependencies<IAggregateRootState>(NullStandardLogger.Instance, mockStateRepo.Object, mockStreamIdBuilder.Object, mockStreamClientFactory.Object, null);
			var ar = new TestAggregateRoot(dependencies, null, aggregateRootName);

			mockCommandValidationResultForSemantics.Setup(x => x.IsValid).Returns(true);
			mockCommand.Setup(x => x.ValidateSemantics()).Returns(mockCommandValidationResultForSemantics.Object);
			mockCommand.Setup(x => x.GetRegionId()).Returns(string.Empty);
			mockCommand.Setup(x => x.GetAggregateRootId()).Returns(string.Empty);
			mockCommand.Setup(x => x.GetCommandId()).Returns(commandId);
			mockStreamIdBuilder.Setup(x => x.Build(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(streamId);
			mockStreamClientFactory.Setup( x => x.Create(It.IsAny<string>())).Returns(mockStreamClient.Object);
			mockStreamClient.Setup(x => x.FirstPositionInStream).Returns(0);
			mockStateRepo.Setup(x => x.LoadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockState.Object);
			mockState.Setup(x => x.IsCausalIdInHistoryAsync(commandId)).ReturnsAsync(false);
			mockCommandResult.Setup(x => x.IsSuccess).Returns(false);
			mockCommandResult.Setup(x => x.Errors).Returns(errors);

			ar.HandleDelegate = (_1, _2, _3) => Task.FromResult<ICommandResult>(mockCommandResult.Object);

			var result = await ar.HandleGenericCommandAsync(mockCommand.Object, cancelSource.Token);

			Assert.False(result.IsSuccess);
			Assert.Contains(error, result.Errors);
		}

		[Fact]
		public async Task process_command_to_committed_events_and_return_success_handler_result_when_handle_generic_command()
		{
			var regionId = "x";
			var aggregateRootName = "ar";
			var aggregateRootId = "1";
			var streamId = "sId";
			var commandId = "cId";
			var mockEvent = new Mock<IBusinessEvent>();
			var events = new List<IBusinessEvent>() { mockEvent.Object }.ToImmutableList();
			var eventType = mockEvent.Object.GetType().Name;
			var eventDataText = "{}";
			var eventDataBytes = Encoding.Unicode.GetBytes(eventDataText);
			var unresolvedEvent = new UnresolvedBusinessEvent(eventType, eventDataBytes);
			var firstPositionInStream = 1;
			var hydratedStreamEvent = new StreamEvent(streamId, firstPositionInStream, null, "whatever", new byte[] { });
			var commitResult = CommitResult.Success;

			var mockCommand = new Mock<ICommand>();
			var mockCommandValidationResult = new Mock<ICommandValidationResult>();
			var mockStreamIdBuilder = new Mock<IStreamIdBuilder>();
			var mockStreamClientFactory = new Mock<IStreamClientFactory>();
			var mockStreamClient = new Mock<IStreamClient>();
			var mockStateRepo = new Mock<IAggregateRootStateRepo<IAggregateRootState>>();
			var mockState = new Mock<IAggregateRootState>();
			var mockCommandResult = new Mock<ICommandResult>();
			var mockResolver = new Mock<IBusinessEventResolver>();
			var cancelSource = new CancellationTokenSource();

			var dependencies = new AggregateRootDependencies<IAggregateRootState>(NullStandardLogger.Instance, mockStateRepo.Object, mockStreamIdBuilder.Object, mockStreamClientFactory.Object, mockResolver.Object);
			var ar = new TestAggregateRoot(dependencies, null, aggregateRootName);

			mockCommandValidationResult.Setup(x => x.IsValid).Returns(true);
			mockCommand.Setup(x => x.ValidateSemantics()).Returns(mockCommandValidationResult.Object);
			mockCommand.Setup(x => x.GetRegionId()).Returns(regionId);
			mockCommand.Setup(x => x.GetAggregateRootId()).Returns(aggregateRootId);
			mockCommand.Setup(x => x.GetCommandId()).Returns(commandId);
			mockStreamIdBuilder.Setup(x => x.Build(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(streamId);
			mockStateRepo.Setup(x => x.LoadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockState.Object);
			mockState.Setup(x => x.IsCausalIdInHistoryAsync(commandId)).ReturnsAsync(false);
			mockState.Setup(x => x.StreamPositionCheckpoint).Returns(firstPositionInStream);
			mockCommandResult.Setup(x => x.IsSuccess).Returns(true);
			mockCommandResult.Setup(x => x.Events).Returns(events);
			mockResolver.Setup(x => x.CanUnresolve(mockEvent.Object)).Returns(true);
			mockResolver.Setup(x => x.Unresolve(mockEvent.Object)).Returns(unresolvedEvent);
			mockStreamClientFactory.Setup(x => x.Create(regionId)).Returns(mockStreamClient.Object);
			mockStreamClient.Setup(x => x.CommitEventsToStreamAsync(It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<IEnumerable<CommitEvent>>())).ReturnsAsync(commitResult);

			ar.HandleDelegate = (pState, pCommand, pToken) =>
			{
				if (pState != mockState.Object || pCommand != mockCommand.Object || pToken != cancelSource.Token)
				{
					throw new Exception();
				}
				return Task.FromResult<ICommandResult>(mockCommandResult.Object);
			};

			var result = await ar.HandleGenericCommandAsync(mockCommand.Object, cancelSource.Token);

			mockStreamClient.Verify(x => x.CommitEventsToStreamAsync(streamId, firstPositionInStream, It.IsAny<IEnumerable<CommitEvent>>()));
			Assert.True(result.IsSuccess);
			Assert.Contains(mockEvent.Object, result.Events);
		}

		[Fact]
		public async Task do_nothing_with_empty_events_and_return_true_when_commit_events()
		{
			var ar = new TestAggregateRoot(new AggregateRootDependencies<IAggregateRootState>(null, null, null, null, null), null, null);

			// Will throw null reference if attempts to process events because dependencies are null.
			var result = await ar.TestCommitEventsAsync(ImmutableList<IBusinessEvent>.Empty, null, null, null);

			Assert.True(result);
		}

		[Fact]
		public async Task throw_when_can_not_unresolve_event_when_commit_events()
		{
			var mockEvent = new Mock<IBusinessEvent>();
			var events = new List<IBusinessEvent>() { mockEvent.Object }.ToImmutableList();
			var mockResolver = new Mock<IBusinessEventResolver>();

			var ar = new TestAggregateRoot(new AggregateRootDependencies<IAggregateRootState>(null, null, null, null, mockResolver.Object), null, null);

			mockResolver.Setup(x => x.CanUnresolve(mockEvent.Object)).Returns(false);

			await Assert.ThrowsAsync<InvalidOperationException>(() => ar.TestCommitEventsAsync(events, null, null, null));
		}

		[Fact]
		public async Task return_false_when_concurrency_conflict_when_commit_events()
		{
			var mockEvent = new Mock<IBusinessEvent>();
			var events = new List<IBusinessEvent>() { mockEvent.Object }.ToImmutableList();
			var eventType = mockEvent.Object.GetType().Name;
			var dataText = "{}";
			var dataBytes = Encoding.Unicode.GetBytes(dataText);
			var unresolvedEvent = new UnresolvedBusinessEvent(eventType, dataBytes);

			var mockResolver = new Mock<IBusinessEventResolver>();
			var mockState = new Mock<IAggregateRootState>();
			var mockStreamClientFactory = new Mock<IStreamClientFactory>();
			var mockStreamClient = new Mock<IStreamClient>();

			var ar = new TestAggregateRoot(new AggregateRootDependencies<IAggregateRootState>(null, null, null, mockStreamClientFactory.Object, mockResolver.Object), null, null);

			mockResolver.Setup(x => x.CanUnresolve(mockEvent.Object)).Returns(true);
			mockResolver.Setup(x => x.Unresolve(mockEvent.Object)).Returns(unresolvedEvent);
			mockStreamClientFactory.Setup(x => x.Create(It.IsAny<string>())).Returns(mockStreamClient.Object);
			mockStreamClient.Setup(x => x.CommitEventsToStreamAsync(It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<IEnumerable<CommitEvent>>())).ReturnsAsync(CommitResult.ConcurrencyConflict);

			var result = await ar.TestCommitEventsAsync(events, null, null, null);

			Assert.False(result);
		}

		[Fact]
		public async Task commit_and_return_true_when_commit_events()
		{
			var regionId = "x";
			var streamId = "s";
			var streamPositionCheckpoint = 0;
			var mockEvent = new Mock<IBusinessEvent>();
			var events = new List<IBusinessEvent>() { mockEvent.Object }.ToImmutableList();
			var eventType = mockEvent.Object.GetType().Name;
			var dataText = "{}";
			var dataBytes = Encoding.Unicode.GetBytes(dataText);
			var unresolvedEvent = new UnresolvedBusinessEvent(eventType, dataBytes);

			var mockResolver = new Mock<IBusinessEventResolver>();
			var mockStreamClientFactory = new Mock<IStreamClientFactory>();
			var mockStreamClient = new Mock<IStreamClient>();

			var ar = new TestAggregateRoot(new AggregateRootDependencies<IAggregateRootState>(null, null, null, mockStreamClientFactory.Object, mockResolver.Object), null, null);

			mockResolver.Setup(x => x.CanUnresolve(mockEvent.Object)).Returns(true);
			mockResolver.Setup(x => x.Unresolve(mockEvent.Object)).Returns(unresolvedEvent);
			mockStreamClientFactory.Setup(x => x.Create(regionId)).Returns(mockStreamClient.Object);
			mockStreamClient.Setup(x => x.CommitEventsToStreamAsync(It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<IEnumerable<CommitEvent>>())).ReturnsAsync(CommitResult.Success);

			var result = await ar.TestCommitEventsAsync(events, regionId, streamId, streamPositionCheckpoint);

			Assert.True(result);

			mockStreamClient.Verify(x =>
				x.CommitEventsToStreamAsync(
					streamId, streamPositionCheckpoint,
					It.Is<IEnumerable<CommitEvent>>(ces => ces.FirstOrDefault(ce => ce.EventType == eventType && Encoding.Unicode.GetString(ce.Data) == dataText) != null)
					)
				);
		}
	}
}
