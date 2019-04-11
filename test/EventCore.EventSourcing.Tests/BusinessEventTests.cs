using Xunit;

namespace EventCore.EventSourcing.Tests
{
	public class BusinessEventTests
	{
		[Fact]
		public void construct()
		{
			var md = BusinessEventMetadata.Empty;

			var e = new BusinessEvent(md);

			Assert.Equal(md, e.Metadata);
		}
	}
}
