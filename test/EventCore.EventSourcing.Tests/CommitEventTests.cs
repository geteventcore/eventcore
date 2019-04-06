using System;
using System.Diagnostics;
using System.Text;
using Xunit;

namespace EventCore.EventSourcing.Tests
{
	public class CommitEventTests
	{
		[Fact]
		public void construct()
		{
			var eventType = "a";
			var data = Encoding.UTF8.GetBytes("{}");

			var e = new CommitEvent(eventType, data);

			Assert.Equal(eventType, e.EventType);
			Assert.Equal(data, e.Data); // Note this is reference equality, not value eq.
		}
	}
}
