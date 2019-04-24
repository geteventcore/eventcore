using Xunit;

namespace EventCore.EventSourcing.Tests
{
	public class BusinessEventTests
	{
		private class TestBusinessEvent : BusinessEvent
		{
			public TestBusinessEvent(BusinessEventMetadata metadata) : base(metadata) { }
		}

		[Fact]
		public void construct()
		{
			var md = BusinessEventMetadata.Empty;

			var e = new TestBusinessEvent(md);

			Assert.Equal(md, e.Metadata);
		}
	}
}
