using EventCore.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace EventCore.EventSourcing.Tests
{
	public class BusinessEventResolverTests
	{
		private class TestBusinessEvent1 : IBusinessEvent { }
		private class TestBusinessEvent2 : IBusinessEvent { }

		private class ErrorBusinessEvent : IBusinessEvent
		{
			private string _fieldWithError;
			public string FieldWithError { get => throw new InvalidOperationException("Test"); set { _fieldWithError = value; } }
			public ErrorBusinessEvent(string fieldWithError) => _fieldWithError = fieldWithError;
		}

		private class NotABusinessEvent { }

		[Fact]
		public void throw_when_construct_with_non_business_event()
		{
			var types = new HashSet<Type>() { typeof(NotABusinessEvent) };
			Assert.Throws<ArgumentException>(() => new BusinessEventResolver(NullStandardLogger.Instance, types)); ;
		}

		[Fact]
		public void can_resolve_true_when_contains_type_name_case_insensitive()
		{
			var types = new HashSet<Type>() { typeof(TestBusinessEvent1) };
			var resolver = new BusinessEventResolver(NullStandardLogger.Instance, types);
			Assert.True(resolver.CanResolve("testBusinessEVENT1"));
		}

		[Fact]
		public void can_resolve_false_when_not_contains_type_name()
		{
			var types = new HashSet<Type>() { typeof(TestBusinessEvent1) };
			var resolver = new BusinessEventResolver(NullStandardLogger.Instance, types);
			Assert.False(resolver.CanResolve("x"));
		}

		[Fact]
		public void can_unresolve_true_when_contains_strong_type()
		{
			var types = new HashSet<Type>() { typeof(TestBusinessEvent1) };
			var resolver = new BusinessEventResolver(NullStandardLogger.Instance, types);
			var e = new TestBusinessEvent1();
			Assert.True(resolver.CanUnresolve(e));
		}

		[Fact]
		public void can_unresolve_false_when_not_contains_strong_type()
		{
			var types = new HashSet<Type>() { typeof(TestBusinessEvent1) };
			var resolver = new BusinessEventResolver(NullStandardLogger.Instance, types);
			var e = new TestBusinessEvent2();
			Assert.False(resolver.CanUnresolve(e));
		}

		[Fact]
		public void throw_when_resolve_and_type_name_not_resolveable()
		{
			var types = new HashSet<Type>() { typeof(TestBusinessEvent1) };
			var resolver = new BusinessEventResolver(NullStandardLogger.Instance, types);
			Assert.ThrowsAny<Exception>(() => resolver.Resolve("x", new byte[] { }));
		}

		[Fact]
		public void resolve_to_null_when_data_not_deserializable()
		{
			var types = new HashSet<Type>() { typeof(TestBusinessEvent1) };
			var resolver = new BusinessEventResolver(NullStandardLogger.Instance, types);
			var e = resolver.Resolve(typeof(TestBusinessEvent1).Name, Encoding.Unicode.GetBytes("/"));
			Assert.Null(e);
		}

		[Fact]
		public void throw_when_unresolve_and_strong_type_not_unresolveable()
		{
			var types = new HashSet<Type>() { typeof(TestBusinessEvent1) };
			var resolver = new BusinessEventResolver(NullStandardLogger.Instance, types);
			var e = new TestBusinessEvent2();
			Assert.ThrowsAny<Exception>(() => resolver.Unresolve(e));
		}

		[Fact]
		public void unresolve_to_null_when_data_not_deserializable()
		{
			var types = new HashSet<Type>() { typeof(ErrorBusinessEvent) };
			var resolver = new BusinessEventResolver(NullStandardLogger.Instance, types);
			var e = new ErrorBusinessEvent("test value");
			var unresolvedEvent = resolver.Unresolve(e);
			Assert.Null(unresolvedEvent);
		}

		[Fact]
		public void resolve_to_business_event()
		{
			var types = new HashSet<Type>() { typeof(TestBusinessEvent1) };
			var resolver = new BusinessEventResolver(NullStandardLogger.Instance, types);
			var e = new TestBusinessEvent1();
			var data = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(e));
			var resolvedEvent = resolver.Resolve(typeof(TestBusinessEvent1).Name, data);
			Assert.NotNull(resolvedEvent);
		}

		[Fact]
		public void unresolve_to_unresolved_business_event()
		{
			var types = new HashSet<Type>() { typeof(TestBusinessEvent1) };
			var resolver = new BusinessEventResolver(NullStandardLogger.Instance, types);
			var e = new TestBusinessEvent1();
			var expectedData = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(e));
			var unresolvedEvent = resolver.Unresolve(e);

			Assert.NotNull(unresolvedEvent);
			Assert.Equal(typeof(TestBusinessEvent1).Name, unresolvedEvent.EventType);
			Assert.NotEmpty(unresolvedEvent.Data);
			Assert.Equal(expectedData.Length, unresolvedEvent.Data.Length);
		}
	}
}
