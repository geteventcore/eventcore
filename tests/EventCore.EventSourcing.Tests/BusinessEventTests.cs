using Xunit;

namespace EventCore.EventSourcing.Tests
{
	public class BusinessEventTests
	{
		private class TestBusinessEvent : BusinessEvent
		{
			public TestBusinessEvent(BusinessEventMetadata _metadata) : base(_metadata) { }
		}

		[Fact]
		public void construct()
		{
			var md = BusinessEventMetadata.Empty;
			var e = new TestBusinessEvent(md);
			Assert.Equal(md, e._Metadata);
		}
	}
}
