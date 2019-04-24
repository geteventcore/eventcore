using System;
using Xunit;

namespace EventCore.AggregateRoots.Tests
{
	public class CommandMetadataTests
	{
		[Fact]
		public void construct()
		{
			var commandId = "1";
			var metadata = new CommandMetadata(commandId);
			Assert.Equal(commandId, metadata.CommandId);
			Assert.InRange(metadata.TimestampUtc, DateTime.MinValue, DateTime.UtcNow);
		}
	}
}
