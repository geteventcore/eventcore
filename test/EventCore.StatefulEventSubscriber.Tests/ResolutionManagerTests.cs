using EventCore.EventSourcing;
using EventCore.Utilities;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventCore.StatefulEventSubscriber.Tests
{
	public class ResolutionManagerTests
	{
		[Fact]
		public async Task stream_eligible_for_resolution_when_stream_state_exists()
		{
			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
			var mockResolver = new Mock<IBusinessEventResolver>();
			var streamId = "s";
			var newPosition = 1;
			var eventType = "x";
			var lastAttemptedPosition = 0;
			var firstPositionInStream = 0;
			var streamState = new StreamState(lastAttemptedPosition, false);
			var manager = new ResolutionManager(NullStandardLogger.Instance, mockResolver.Object, mockStreamStateRepo.Object, null, null);
			var streamEvent = new StreamEvent(streamId, newPosition, null, eventType, new byte[] { });

			mockStreamStateRepo.Setup(x => x.LoadStreamStateAsync(streamId)).ReturnsAsync(streamState);
			mockResolver.Setup(x => x.CanResolve(eventType)).Returns(true);

			var eligibility = await manager.IsStreamEventEligibleForResolution(streamEvent, firstPositionInStream);

			Assert.Equal(ResolutionEligibility.Eligible, eligibility);
		}

		[Fact]
		public async Task stream_eligible_for_resolution_when_stream_state_not_exists()
		{
			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
			var streamId = "s";
			var newPosition = 0; // Must be same as first position for this test.
			var eventType = "x";
			var firstPositionInStream = 0;
			var manager = new ResolutionManager(NullStandardLogger.Instance, null, mockStreamStateRepo.Object, null, null);
			var streamEvent = new StreamEvent(streamId, newPosition, null, eventType, new byte[] { });

			mockStreamStateRepo.Setup(x => x.LoadStreamStateAsync(streamId)).ReturnsAsync((StreamState)null);

			var eligibility = await manager.IsStreamEventEligibleForResolution(streamEvent, firstPositionInStream);

			Assert.Equal(ResolutionEligibility.Eligible, eligibility);
		}

		[Fact]
		public async Task stream_ineligible_for_resolution_skipped_already_processed()
		{
			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
			var streamId = "s";
			var newPosition = 1;
			var eventType = "x";
			var lastAttemptedPosition = newPosition + 1; // Must be greater than new position for this test.
			var firstPositionInStream = 0;
			var streamState = new StreamState(lastAttemptedPosition, false);
			var manager = new ResolutionManager(NullStandardLogger.Instance, null, mockStreamStateRepo.Object, null, null);
			var streamEvent = new StreamEvent(streamId, newPosition, null, eventType, new byte[] { });

			mockStreamStateRepo.Setup(x => x.LoadStreamStateAsync(streamId)).ReturnsAsync(streamState);

			var eligibility = await manager.IsStreamEventEligibleForResolution(streamEvent, firstPositionInStream);

			Assert.Equal(ResolutionEligibility.SkippedAlreadyProcessed, eligibility);
		}

		[Fact]
		public async Task stream_ineligible_for_resolution_unable_has_stream_error()
		{
			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
			var streamId = "s";
			var newPosition = 1;
			var eventType = "x";
			var lastAttemptedPosition = 0;
			var firstPositionInStream = 0;
			var streamState = new StreamState(lastAttemptedPosition, true);
			var manager = new ResolutionManager(NullStandardLogger.Instance, null, mockStreamStateRepo.Object, null, null);
			var streamEvent = new StreamEvent(streamId, newPosition, null, eventType, new byte[] { });

			mockStreamStateRepo.Setup(x => x.LoadStreamStateAsync(streamId)).ReturnsAsync(streamState);

			var eligibility = await manager.IsStreamEventEligibleForResolution(streamEvent, firstPositionInStream);

			Assert.Equal(ResolutionEligibility.UnableStreamHasError, eligibility);
		}

		[Fact]
		public async Task stream_ineligible_for_resolution_unable_to_resolve_event_type()
		{
			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
			var mockResolver = new Mock<IBusinessEventResolver>();
			var streamId = "s";
			var newPosition = 1;
			var eventType = "x";
			var lastAttemptedPosition = 0;
			var firstPositionInStream = 0;
			var streamState = new StreamState(lastAttemptedPosition, false);
			var manager = new ResolutionManager(NullStandardLogger.Instance, mockResolver.Object, mockStreamStateRepo.Object, null, null);
			var streamEvent = new StreamEvent(streamId, newPosition, null, eventType, new byte[] { });

			mockStreamStateRepo.Setup(x => x.LoadStreamStateAsync(streamId)).ReturnsAsync(streamState);
			mockResolver.Setup(x => x.CanResolve(eventType)).Returns(false);

			var eligibility = await manager.IsStreamEventEligibleForResolution(streamEvent, firstPositionInStream);

			Assert.Equal(ResolutionEligibility.UnableToResolveEventType, eligibility);
		}

		[Fact]
		public async Task stream_eligible_for_resolution_throws_non_sequential_position_and_stream_state_not_exists()
		{
			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
			var streamId = "s";
			var newPosition = 1; // Must be first position in stream + 1 for this test.
			var eventType = "x";
			var firstPositionInStream = 0;
			var manager = new ResolutionManager(NullStandardLogger.Instance, null, mockStreamStateRepo.Object, null, null);
			var streamEvent = new StreamEvent(streamId, newPosition, null, eventType, new byte[] { });

			mockStreamStateRepo.Setup(x => x.LoadStreamStateAsync(streamId)).ReturnsAsync((StreamState)null);

			await Assert.ThrowsAsync<InvalidOperationException>(() => manager.IsStreamEventEligibleForResolution(streamEvent, firstPositionInStream));
		}

		[Fact]
		public async Task stream_eligible_for_resolution_throws_non_sequential_position_and_stream_state_exists()
		{
			var mockStreamStateRepo = new Mock<IStreamStateRepo>();
			var mockResolver = new Mock<IBusinessEventResolver>();
			var streamId = "s";
			var newPosition = 2; // Must be greater than last attempted position + 1 for this test.
			var eventType = "x";
			var lastAttemptedPosition = 0;
			var firstPositionInStream = 0;
			var streamState = new StreamState(lastAttemptedPosition, false);
			var manager = new ResolutionManager(NullStandardLogger.Instance, mockResolver.Object, mockStreamStateRepo.Object, null, null);
			var streamEvent = new StreamEvent(streamId, newPosition, null, eventType, new byte[] { });

			mockStreamStateRepo.Setup(x => x.LoadStreamStateAsync(streamId)).ReturnsAsync(streamState);
			mockResolver.Setup(x => x.CanResolve(eventType)).Returns(true);

			await Assert.ThrowsAsync<InvalidOperationException>(() => manager.IsStreamEventEligibleForResolution(streamEvent, firstPositionInStream));
		}
	}
}
