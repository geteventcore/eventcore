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
			var payload = Encoding.UTF8.GetBytes("{}");

			var e = new CommitEvent(eventType, payload);

			Assert.Equal(e.EventType, eventType);
			Assert.Equal(e.Payload, payload); // Note this is reference equality, not value eq.
		}
	}
}
