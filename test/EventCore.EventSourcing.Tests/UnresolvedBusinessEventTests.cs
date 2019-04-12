using System.Text;
using Xunit;

namespace EventCore.EventSourcing.Tests
{
	public class UnresolvedBusinessEventTests
	{
		[Fact]
		public void construct()
		{
			var eventType = "a";
			var data = Encoding.UTF8.GetBytes("{}");

			var e = new UnresolvedBusinessEvent(eventType, data);

			Assert.Equal(eventType, e.EventType);
			Assert.Equal(data, e.Data);// Reference equality, not value eq.
		}
	}
}
