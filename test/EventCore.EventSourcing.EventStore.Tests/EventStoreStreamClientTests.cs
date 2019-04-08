using EventCore.Utilities;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using EventStore.ClientAPI.SystemData;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventCore.EventSourcing.EventStore.Tests
{

	public class EventStoreStreamClientTests
	{
		private byte[] EmptyJsonPayload { get => Encoding.Unicode.GetBytes("{}"); }
		private EventStoreStreamClientOptions ClientOptions { get => new EventStoreStreamClientOptions(100); }

		private class TestException : Exception { }

		private const string DEFAULT_REGION_ID = "x";

		// Use reflection to force build of class with internal constructor.
		private StreamEventsSlice ForceCreateStreamEventsSlice(
			SliceReadStatus status, string streamId, ResolvedEvent[] events,
			long nextEventNumber,
			bool isEndOfStream
		)
		{
			var slice = (StreamEventsSlice)FormatterServices.GetUninitializedObject(typeof(StreamEventsSlice));
			typeof(StreamEventsSlice).GetField("Status", BindingFlags.Instance | BindingFlags.Public).SetValue(slice, status);
			typeof(StreamEventsSlice).GetField("Stream", BindingFlags.Instance | BindingFlags.Public).SetValue(slice, streamId);
			typeof(StreamEventsSlice).GetField("Events", BindingFlags.Instance | BindingFlags.Public).SetValue(slice, events);
			typeof(StreamEventsSlice).GetField("NextEventNumber", BindingFlags.Instance | BindingFlags.Public).SetValue(slice, nextEventNumber);
			typeof(StreamEventsSlice).GetField("IsEndOfStream", BindingFlags.Instance | BindingFlags.Public).SetValue(slice, isEndOfStream);
			return slice;
		}

		private ResolvedEvent ForceCreateResolvedEvent(RecordedEvent recordedEvent, RecordedEvent linkEvent = null)
		{
			// Create object without constructor.
			// SetValue for struct must be boxed.
			var x = (ResolvedEvent)FormatterServices.GetUninitializedObject(typeof(ResolvedEvent));
			object boxed = x;
			typeof(ResolvedEvent).GetField("Event", BindingFlags.Instance | BindingFlags.Public).SetValue(boxed, recordedEvent);
			typeof(ResolvedEvent).GetField("Link", BindingFlags.Instance | BindingFlags.Public).SetValue(boxed, linkEvent);
			x = (ResolvedEvent)boxed;
			return x;
		}

		private RecordedEvent ForceCreateRecordedEvent(string eventStreamId, long eventNumber, string eventType, byte[] data)
		{
			// Create object without constructor.
			var x = (RecordedEvent)FormatterServices.GetUninitializedObject(typeof(RecordedEvent));
			typeof(RecordedEvent).GetField("EventStreamId", BindingFlags.Instance | BindingFlags.Public).SetValue(x, eventStreamId);
			typeof(RecordedEvent).GetField("EventNumber", BindingFlags.Instance | BindingFlags.Public).SetValue(x, eventNumber);
			typeof(RecordedEvent).GetField("EventType", BindingFlags.Instance | BindingFlags.Public).SetValue(x, eventType);
			typeof(RecordedEvent).GetField("Data", BindingFlags.Instance | BindingFlags.Public).SetValue(x, data);
			return x;
		}

		private EventReadResult ForceCreateEventReadResult(EventReadStatus status, ResolvedEvent resolvedEvent)
		{
			// Create object without constructor.
			var x = (EventReadResult)FormatterServices.GetUninitializedObject(typeof(EventReadResult));
			typeof(EventReadResult).GetField("Status", BindingFlags.Instance | BindingFlags.Public).SetValue(x, status);
			typeof(EventReadResult).GetField("Event", BindingFlags.Instance | BindingFlags.Public).SetValue(x, resolvedEvent);
			return x;
		}

		private EventStoreStreamCatchUpSubscription ForceCreateEventStoreStreamCatchUpSubscription()
		{
			// Create object without constructor.
			var x = (EventStoreStreamCatchUpSubscription)FormatterServices.GetUninitializedObject(typeof(EventStoreStreamCatchUpSubscription));
			return x;
		}


		[Fact]
		public async Task commit_should_detect_invalid_stream_id()
		{
			var client = new EventStoreStreamClient(NullStandardLogger.Instance, new Mock<IEventStoreConnectionFactory>().Object, ClientOptions);
			var invalidStreamId = "s!"; // Contains invalid char.

			await Assert.ThrowsAsync<ArgumentException>(() => client.CommitEventsToStreamAsync(DEFAULT_REGION_ID, invalidStreamId, null, new CommitEvent[] { }));
		}

		[Fact]
		public async Task commit_should_throw_when_invalid_expected_position()
		{
			var client = new EventStoreStreamClient(NullStandardLogger.Instance, new Mock<IEventStoreConnectionFactory>().Object, ClientOptions);
			var streamId = "s";
			var invalidPosition = client.FirstPositionInStream - 1;

			await Assert.ThrowsAsync<ArgumentException>(() => client.CommitEventsToStreamAsync(DEFAULT_REGION_ID, streamId, invalidPosition, new CommitEvent[] { }));
		}

		[Fact]
		public async Task commit_should_detect_concurrency_conflict()
		{
			var mockConn = new Mock<IEventStoreConnection>();
			var mockConnFactory = new Mock<IEventStoreConnectionFactory>();
			Func<IEventStoreConnection> connFactory = () => mockConn.Object;
			var client = new EventStoreStreamClient(NullStandardLogger.Instance, mockConnFactory.Object, ClientOptions);
			var streamId = "s";
			var eventType = "a";
			var e = new CommitEvent(eventType, EmptyJsonPayload);

			mockConnFactory.Setup(x => x.Create(DEFAULT_REGION_ID)).Returns(mockConn.Object);

			mockConn
				.Setup(x => x.AppendToStreamAsync(streamId, client.FirstPositionInStream, It.IsAny<IEnumerable<EventData>>(), null))
				.ThrowsAsync(new WrongExpectedVersionException(""));

			var result = await client.CommitEventsToStreamAsync(DEFAULT_REGION_ID, streamId, client.FirstPositionInStream, new CommitEvent[] { e });

			Assert.Equal(CommitResult.ConcurrencyConflict, result);
		}

		[Fact]
		public async Task commit_should_set_expected_version_when_new_stream()
		{
			var mockConn = new Mock<IEventStoreConnection>();
			var mockConnFactory = new Mock<IEventStoreConnectionFactory>();
			Func<IEventStoreConnection> connFactory = () => mockConn.Object;
			var client = new EventStoreStreamClient(NullStandardLogger.Instance, mockConnFactory.Object, ClientOptions);
			var streamId = "s";
			var eventType = "a";
			var e = new CommitEvent(eventType, EmptyJsonPayload);

			mockConnFactory.Setup(x => x.Create(DEFAULT_REGION_ID)).Returns(mockConn.Object);

			await client.CommitEventsToStreamAsync(DEFAULT_REGION_ID, streamId, null, new CommitEvent[] { e });

			mockConn.Verify(x => x.AppendToStreamAsync(It.IsAny<string>(), ExpectedVersion.NoStream, It.IsAny<IEnumerable<EventData>>(), null));
		}

		[Fact]
		public async Task commit_should_set_expected_version_when_existing_stream()
		{
			var mockConn = new Mock<IEventStoreConnection>();
			var mockConnFactory = new Mock<IEventStoreConnectionFactory>();
			var client = new EventStoreStreamClient(NullStandardLogger.Instance, mockConnFactory.Object, ClientOptions);
			var streamId = "s";
			var eventType = "a";
			var e = new CommitEvent(eventType, EmptyJsonPayload);

			mockConnFactory.Setup(x => x.Create(DEFAULT_REGION_ID)).Returns(mockConn.Object);

			await client.CommitEventsToStreamAsync(DEFAULT_REGION_ID, streamId, client.FirstPositionInStream, new CommitEvent[] { e });

			mockConn.Verify(x => x.AppendToStreamAsync(It.IsAny<string>(), client.FirstPositionInStream, It.IsAny<IEnumerable<EventData>>(), null));
		}

		[Fact]
		public async Task commit_should_build_event_data_from_commit_events()
		{
			var mockConn = new Mock<IEventStoreConnection>();
			var mockConnFactory = new Mock<IEventStoreConnectionFactory>();
			var client = new EventStoreStreamClient(NullStandardLogger.Instance, mockConnFactory.Object, ClientOptions);
			var streamId = "s";
			var eventType1 = "a1";
			var eventType2 = "a2";
			var json1 = "{'prop':'val1'}";
			var json2 = "{'prop':'val2'}";
			var e1 = new CommitEvent(eventType1, Encoding.Unicode.GetBytes(json1));
			var e2 = new CommitEvent(eventType2, Encoding.Unicode.GetBytes(json2));

			mockConnFactory.Setup(x => x.Create(DEFAULT_REGION_ID)).Returns(mockConn.Object);

			await client.CommitEventsToStreamAsync(DEFAULT_REGION_ID, streamId, null, new CommitEvent[] { e1, e2 });

			mockConn.Verify(x => x.AppendToStreamAsync(
				It.IsAny<string>(), It.IsAny<long>(),
				It.Is<IEnumerable<EventData>>(events =>
					events.ToList().First().Type == eventType1 && Encoding.Unicode.GetString(events.ToList().First().Data) == json1
					&& events.ToList().Last().Type == eventType2 && Encoding.Unicode.GetString(events.ToList().Last().Data) == json2
				),
				null));
		}

		[Fact]
		public async Task commit_should_succeed_when_events_appended_to_specific_stream()
		{
			var mockConn = new Mock<IEventStoreConnection>();
			var mockConnFactory = new Mock<IEventStoreConnectionFactory>();
			var client = new EventStoreStreamClient(NullStandardLogger.Instance, mockConnFactory.Object, ClientOptions);
			var streamId = "s";
			var eventType = "a";
			var e = new CommitEvent(eventType, EmptyJsonPayload);

			mockConnFactory.Setup(x => x.Create(DEFAULT_REGION_ID)).Returns(mockConn.Object);

			mockConn
				.Setup(x => x.AppendToStreamAsync(streamId, ExpectedVersion.NoStream, It.IsAny<IEnumerable<EventData>>(), null))
				.ReturnsAsync(new WriteResult(0, new Position(0, 0))); // Write result can be anything.

			var result = await client.CommitEventsToStreamAsync(DEFAULT_REGION_ID, streamId, null, new CommitEvent[] { e });

			Assert.Equal(CommitResult.Success, result);
		}

		[Fact]
		public async Task commit_should_error_when_exception_thrown()
		{
			var mockConn = new Mock<IEventStoreConnection>();
			var mockConnFactory = new Mock<IEventStoreConnectionFactory>();
			var client = new EventStoreStreamClient(NullStandardLogger.Instance, mockConnFactory.Object, ClientOptions);
			var streamId = "s";
			var eventType = "a";
			var e = new CommitEvent(eventType, EmptyJsonPayload);

			mockConnFactory.Setup(x => x.Create(DEFAULT_REGION_ID)).Returns(mockConn.Object);

			mockConn
				.Setup(x => x.AppendToStreamAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<EventData>>(), null))
				.ThrowsAsync(new TestException());

			await Assert.ThrowsAsync<TestException>(() => client.CommitEventsToStreamAsync(DEFAULT_REGION_ID, streamId, null, new CommitEvent[] { e }));
		}

		[Fact]
		public async Task load_should_throw_when_invalid_from_position()
		{
			var mockConnFactory = new Mock<IEventStoreConnectionFactory>();
			var client = new EventStoreStreamClient(NullStandardLogger.Instance, mockConnFactory.Object, ClientOptions);
			var streamId = "s";
			var invalidPosition = client.FirstPositionInStream - 1;

			mockConnFactory.Setup(x => x.Create(DEFAULT_REGION_ID)).Returns(new Mock<IEventStoreConnection>().Object);

			await Assert.ThrowsAsync<ArgumentException>(() => client.LoadStreamEventsAsync(DEFAULT_REGION_ID, streamId, invalidPosition, (se, ct) => Task.CompletedTask, CancellationToken.None));
		}

		[Fact]
		public async Task load_should_rethrow_when_reader_exception()
		{
			var mockConn = new Mock<IEventStoreConnection>();
			var mockConnFactory = new Mock<IEventStoreConnectionFactory>();
			var client = new EventStoreStreamClient(NullStandardLogger.Instance, mockConnFactory.Object, ClientOptions);
			var streamId = "s";
			var ex = new TestException();

			mockConnFactory.Setup(x => x.Create(DEFAULT_REGION_ID)).Returns(mockConn.Object);

			mockConn
				.Setup(x => x.ReadStreamEventsForwardAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<bool>(), null))
				.ThrowsAsync(ex);

			await Assert.ThrowsAsync<TestException>(() => client.LoadStreamEventsAsync(DEFAULT_REGION_ID, streamId, client.FirstPositionInStream, (se, ct) => Task.CompletedTask, CancellationToken.None));
		}

		[Fact]
		public async Task load_should_rethrow_when_receiver_exception()
		{
			var mockConn = new Mock<IEventStoreConnection>();
			var mockConnFactory = new Mock<IEventStoreConnectionFactory>();
			var client = new EventStoreStreamClient(NullStandardLogger.Instance, mockConnFactory.Object, ClientOptions);
			var streamId = "s";
			var eventType = "a";
			var json = "{'prop':'val1'}";
			var data = Encoding.Unicode.GetBytes(json);
			var mockEvent = ForceCreateResolvedEvent(ForceCreateRecordedEvent(streamId, client.FirstPositionInStream, eventType, data), null);
			var mockSlice = ForceCreateStreamEventsSlice(SliceReadStatus.Success, streamId, new ResolvedEvent[] { mockEvent }, 0, true);

			var ex = new TestException();

			mockConnFactory.Setup(x => x.Create(DEFAULT_REGION_ID)).Returns(mockConn.Object);

			mockConn
				.Setup(x => x.ReadStreamEventsForwardAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<bool>(), null))
				.ReturnsAsync(mockSlice);

			await Assert.ThrowsAsync<TestException>(() => client.LoadStreamEventsAsync(
				DEFAULT_REGION_ID, streamId, client.FirstPositionInStream,
				(se, ct) => throw new TestException(),
				CancellationToken.None));
		}

		[Fact]
		public async Task load_should_not_throw_when_read_result_not_success()
		{
			var mockConn = new Mock<IEventStoreConnection>();
			var mockConnFactory = new Mock<IEventStoreConnectionFactory>();
			var client = new EventStoreStreamClient(NullStandardLogger.Instance, mockConnFactory.Object, ClientOptions);
			var streamId = "s";

			// Status is the only value that matters here.
			var mockSlice = ForceCreateStreamEventsSlice(SliceReadStatus.StreamNotFound, streamId, new ResolvedEvent[] { }, 0, true);

			mockConnFactory.Setup(x => x.Create(DEFAULT_REGION_ID)).Returns(mockConn.Object);

			mockConn
				.Setup(x => x.ReadStreamEventsForwardAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<bool>(), null))
				.ReturnsAsync(mockSlice);

			await client.LoadStreamEventsAsync(DEFAULT_REGION_ID, streamId, client.FirstPositionInStream, (se, ct) => Task.CompletedTask, CancellationToken.None);
		}

		[Fact]
		public async Task load_should_call_read_stream_forward()
		{
			var mockConn = new Mock<IEventStoreConnection>();
			var mockConnFactory = new Mock<IEventStoreConnectionFactory>();
			var client = new EventStoreStreamClient(NullStandardLogger.Instance, mockConnFactory.Object, ClientOptions);
			var streamId = "s";
			var mockSlice = ForceCreateStreamEventsSlice(SliceReadStatus.Success, streamId, new ResolvedEvent[] { }, 0, true);

			mockConnFactory.Setup(x => x.Create(DEFAULT_REGION_ID)).Returns(mockConn.Object);

			mockConn
				.Setup(x => x.ReadStreamEventsForwardAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<bool>(), null))
				.ReturnsAsync(mockSlice);

			await client.LoadStreamEventsAsync(DEFAULT_REGION_ID, streamId, client.FirstPositionInStream, (se, ct) => Task.CompletedTask, CancellationToken.None);

			mockConn.Verify(x => x.ReadStreamEventsForwardAsync(streamId, client.FirstPositionInStream, It.IsAny<int>(), It.IsAny<bool>(), null));
		}

		[Fact]
		public async Task load_should_call_receiver_with_correct_event_properties()
		{
			var mockConn = new Mock<IEventStoreConnection>();
			var mockConnFactory = new Mock<IEventStoreConnectionFactory>();
			var client = new EventStoreStreamClient(NullStandardLogger.Instance, mockConnFactory.Object, new EventStoreStreamClientOptions(1));
			var streamId = "s";
			var linkStreamId = "l";
			var linkPosition = 20; // This can be whatever.
			var eventType1 = "a1";
			var eventType2 = "a2";
			var json1 = "{'prop':'val1'}";
			var json2 = "{'prop':'val2'}";
			var data1 = Encoding.Unicode.GetBytes(json1);
			var data2 = Encoding.Unicode.GetBytes(json2);
			var mockEvent1 = ForceCreateResolvedEvent(
				ForceCreateRecordedEvent(streamId, client.FirstPositionInStream, eventType1, data1),
				ForceCreateRecordedEvent(linkStreamId, linkPosition, null, null));
			var mockEvent2 = ForceCreateResolvedEvent(ForceCreateRecordedEvent(streamId, client.FirstPositionInStream + 1, eventType2, data2), null);

			var mockSlice1 = ForceCreateStreamEventsSlice(SliceReadStatus.Success, streamId, new ResolvedEvent[] { mockEvent1 }, client.FirstPositionInStream + 1, false);
			var mockSlice2 = ForceCreateStreamEventsSlice(SliceReadStatus.Success, streamId, new ResolvedEvent[] { mockEvent2 }, 0, true);

			mockConnFactory.Setup(x => x.Create(DEFAULT_REGION_ID)).Returns(mockConn.Object);

			var calledCount = 0;

			// First round of events returned.
			mockConn
				.Setup(x => x.ReadStreamEventsForwardAsync(It.IsAny<string>(), It.Is<long>(pos => pos == client.FirstPositionInStream), It.IsAny<int>(), It.IsAny<bool>(), null))
				.ReturnsAsync(mockSlice1);

			// Second round of events returned.
			mockConn
				.Setup(x => x.ReadStreamEventsForwardAsync(It.IsAny<string>(), It.Is<long>(pos => pos == client.FirstPositionInStream + 1), It.IsAny<int>(), It.IsAny<bool>(), null))
				.ReturnsAsync(mockSlice2);

			await client.LoadStreamEventsAsync(
				DEFAULT_REGION_ID, streamId, client.FirstPositionInStream,
				(se, ct) =>
				{
					calledCount++;

					if (se.Position == client.FirstPositionInStream)
					{
						if (
							se.StreamId != streamId || se.EventType != eventType1 || Encoding.Unicode.GetString(se.Data) != json1
							|| !se.IsLink || se.Link == null || se.Link.StreamId != linkStreamId || se.Link.Position != linkPosition
							)
						{
							throw new Exception("Invalid event 1.");
						}
					}

					if (se.Position == client.FirstPositionInStream + 1)
					{
						if (
							se.StreamId != streamId || se.EventType != eventType2 || Encoding.Unicode.GetString(se.Data) != json2
							|| se.IsLink || se.Link != null
							)
						{
							throw new Exception("Invalid event 2.");
						}
					}

					return Task.CompletedTask;
				},
				CancellationToken.None);

			Assert.Equal(2, calledCount);
		}

		[Fact]
		public async Task subscribe_should_throw_when_invalid_from_position()
		{
			var mockConnFactory = new Mock<IEventStoreConnectionFactory>();
			var client = new EventStoreStreamClient(NullStandardLogger.Instance, mockConnFactory.Object, ClientOptions);
			var streamId = "s";
			var invalidPosition = client.FirstPositionInStream - 1;

			mockConnFactory.Setup(x => x.Create(DEFAULT_REGION_ID)).Returns(new Mock<IEventStoreConnection>().Object);

			await Assert.ThrowsAsync<ArgumentException>(() => client.SubscribeToStreamAsync(DEFAULT_REGION_ID, streamId, invalidPosition, (se, ct) => Task.CompletedTask, CancellationToken.None));
		}

		[Fact]
		public async Task subscribe_should_rethrow_when_reader_exception()
		{
			var mockConn = new Mock<IEventStoreConnection>();
			var mockConnFactory = new Mock<IEventStoreConnectionFactory>();
			var client = new EventStoreStreamClient(NullStandardLogger.Instance, mockConnFactory.Object, ClientOptions);
			var streamId = "s";
			var ex = new TestException();

			mockConnFactory.Setup(x => x.Create(DEFAULT_REGION_ID)).Returns(mockConn.Object);

			mockConn
				.Setup(x => x.SubscribeToStreamFrom(
					It.IsAny<string>(), It.IsAny<long>(),
					It.IsAny<CatchUpSubscriptionSettings>(),
					It.IsAny<Func<EventStoreCatchUpSubscription, ResolvedEvent, Task>>(), null,
					It.IsAny<Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception>>(), null))
				.Throws(ex);

			await Assert.ThrowsAsync<TestException>(() => client.SubscribeToStreamAsync(DEFAULT_REGION_ID, streamId, client.FirstPositionInStream, (se, ct) => Task.CompletedTask, new CancellationTokenSource(5000).Token));
		}

		[Fact]
		public async Task subscribe_should_rethrow_when_receiver_exception()
		{
			var mockConn = new Mock<IEventStoreConnection>();
			var mockConnFactory = new Mock<IEventStoreConnectionFactory>();
			var client = new EventStoreStreamClient(NullStandardLogger.Instance, mockConnFactory.Object, ClientOptions);
			var streamId = "s";
			var eventType = "a";
			var json = "{'prop':'val1'}";
			var data = Encoding.Unicode.GetBytes(json);
			var mockEvent = ForceCreateResolvedEvent(ForceCreateRecordedEvent(streamId, client.FirstPositionInStream, eventType, data), null);
			var mockSlice = ForceCreateStreamEventsSlice(SliceReadStatus.Success, streamId, new ResolvedEvent[] { mockEvent }, 0, true);

			var ex = new TestException();

			mockConnFactory.Setup(x => x.Create(DEFAULT_REGION_ID)).Returns(mockConn.Object);

			mockConn
				.Setup(x => x.SubscribeToStreamFrom(
					It.IsAny<string>(), It.IsAny<long>(), It.IsAny<CatchUpSubscriptionSettings>(),
					It.IsAny<Func<EventStoreCatchUpSubscription, ResolvedEvent, Task>>(),
					null, It.IsAny<Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception>>(), null))
				// Callback is to simulate call to receiver.
				.Callback<string, long?, CatchUpSubscriptionSettings, Func<EventStoreCatchUpSubscription, ResolvedEvent, Task>, Action<EventStoreCatchUpSubscription>, Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception>, UserCredentials>(
					(_1, _2, _3, receiverAsync, _5, _6, _7) =>
					{
						try
						{
							receiverAsync(null, ForceCreateResolvedEvent(ForceCreateRecordedEvent(null, 0, null, null), null)).Wait();
						}
						catch (AggregateException aggEx)
						{
							// For some reason this method doesn't unwrap the aggregate exception despite the use of Wait().
							throw aggEx.InnerException;
						}
					})
				.Returns(ForceCreateEventStoreStreamCatchUpSubscription());

			var cancelSource = new CancellationTokenSource(5000); // Make sure the subscriber is cancelled after timeout.

			await Assert.ThrowsAsync<TestException>(() => client.SubscribeToStreamAsync(
				DEFAULT_REGION_ID, streamId, client.FirstPositionInStream,
				(se, ct) => throw new TestException(),
				cancelSource.Token));

			if (cancelSource.IsCancellationRequested)
				throw new TimeoutException();
		}

		[Fact]
		public async Task subscribe_should_call_receiver_with_correct_event_properties()
		{
			var mockConn = new Mock<IEventStoreConnection>();
			var mockConnFactory = new Mock<IEventStoreConnectionFactory>();
			var client = new EventStoreStreamClient(NullStandardLogger.Instance, mockConnFactory.Object, new EventStoreStreamClientOptions(1));
			var streamId = "s";
			var linkStreamId = "l";
			var linkPosition = 20; // This can be whatever.
			var eventType1 = "a1";
			var eventType2 = "a2";
			var json1 = "{'prop':'val1'}";
			var json2 = "{'prop':'val2'}";
			var data1 = Encoding.Unicode.GetBytes(json1);
			var data2 = Encoding.Unicode.GetBytes(json2);
			var mockEvent1 = ForceCreateResolvedEvent(
				ForceCreateRecordedEvent(streamId, client.FirstPositionInStream, eventType1, data1),
				ForceCreateRecordedEvent(linkStreamId, linkPosition, null, null));
			var mockEvent2 = ForceCreateResolvedEvent(ForceCreateRecordedEvent(streamId, client.FirstPositionInStream + 1, eventType2, data2), null);

			mockConnFactory.Setup(x => x.Create(DEFAULT_REGION_ID)).Returns(mockConn.Object);

			var calledCount = 0;

			// First round of events returned.
			mockConn
				.Setup(x => x.SubscribeToStreamFrom(
					streamId, It.Is<long>(pos => pos == client.FirstPositionInStream),
					It.IsAny<CatchUpSubscriptionSettings>(), It.IsAny<Func<EventStoreCatchUpSubscription, ResolvedEvent, Task>>(),
					null, It.IsAny<Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception>>(), null))
				// Callback is to simulate call to receiver.
				.Callback<string, long?, CatchUpSubscriptionSettings, Func<EventStoreCatchUpSubscription, ResolvedEvent, Task>, Action<EventStoreCatchUpSubscription>, Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception>, UserCredentials>(
					(_1, _2, _3, receiverAsync, _5, _6, _7) =>
					{
						try
						{
							receiverAsync(null, mockEvent1).Wait();
							receiverAsync(null, mockEvent2).Wait();
						}
						catch (AggregateException aggEx)
						{
							// For some reason this method doesn't unwrap the aggregate exception despite the use of Wait().
							throw aggEx.InnerException;
						}
					})
				.Returns(ForceCreateEventStoreStreamCatchUpSubscription());

			var cancelSource = new CancellationTokenSource(5000);// Make sure the subscriber is cancelled after timeout.
			var mutex = new ManualResetEventSlim(false);

			await client.SubscribeToStreamAsync(
				DEFAULT_REGION_ID, streamId, client.FirstPositionInStream,
				(se, ct) =>
				{
					calledCount++;

					if (se.Position == client.FirstPositionInStream)
					{
						if (
							se.StreamId != streamId || se.EventType != eventType1 || Encoding.Unicode.GetString(se.Data) != json1
							|| !se.IsLink || se.Link == null || se.Link.StreamId != linkStreamId || se.Link.Position != linkPosition
							)
						{
							throw new Exception("Invalid event 1.");
						}
					}

					if (se.Position == client.FirstPositionInStream + 1)
					{
						if (
							se.StreamId != streamId || se.EventType != eventType2 || Encoding.Unicode.GetString(se.Data) != json2
							|| se.IsLink || se.Link != null
							)
						{
							throw new Exception("Invalid event 2.");
						}
					}

					if (calledCount == 2) mutex.Set();

					return Task.CompletedTask;
				},
				cancelSource.Token);

			await Task.WhenAny(new[] { cancelSource.Token.WaitHandle.AsTask(), mutex.WaitHandle.AsTask() });

			if (!mutex.IsSet && cancelSource.IsCancellationRequested)
				throw new TimeoutException();

			Assert.Equal(2, calledCount);
		}

		[Fact]
		public async Task find_last_position_in_stream_should_be_null_when_no_stream()
		{
			var mockConn = new Mock<IEventStoreConnection>();
			var mockConnFactory = new Mock<IEventStoreConnectionFactory>();
			var client = new EventStoreStreamClient(NullStandardLogger.Instance, mockConnFactory.Object, ClientOptions);
			var streamId = "s";

			var mockReadResult = ForceCreateEventReadResult(EventReadStatus.NotFound, ForceCreateResolvedEvent(ForceCreateRecordedEvent(null, 0, null, null), null));

			mockConnFactory.Setup(x => x.Create(DEFAULT_REGION_ID)).Returns(mockConn.Object);

			mockConn
				.Setup(x => x.ReadEventAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<bool>(), null))
				.ReturnsAsync(mockReadResult);

			var position = await client.FindLastPositionInStreamAsync(DEFAULT_REGION_ID, streamId);

			Assert.Null(position);
		}

		[Fact]
		public async Task find_last_position_in_stream_should_be_correct_value()
		{
			var mockConn = new Mock<IEventStoreConnection>();
			var mockConnFactory = new Mock<IEventStoreConnectionFactory>();
			var client = new EventStoreStreamClient(NullStandardLogger.Instance, mockConnFactory.Object, ClientOptions);
			var streamId = "s";
			var expectedPosition = 143; // Made up number.

			var mockReadResult = ForceCreateEventReadResult(EventReadStatus.Success, ForceCreateResolvedEvent(ForceCreateRecordedEvent(streamId, expectedPosition, null, null), null));

			mockConnFactory.Setup(x => x.Create(DEFAULT_REGION_ID)).Returns(mockConn.Object);

			mockConn
				.Setup(x => x.ReadEventAsync(streamId, StreamPosition.End, false, null)) // Check all values.
				.ReturnsAsync(mockReadResult);

			var actualPosition = await client.FindLastPositionInStreamAsync(DEFAULT_REGION_ID, streamId);

			Assert.Equal(expectedPosition, actualPosition);
		}
	}
}
