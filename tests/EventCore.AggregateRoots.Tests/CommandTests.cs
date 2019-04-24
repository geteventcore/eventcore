using System;
using Xunit;

namespace EventCore.AggregateRoots.Tests
{
	public class CommandTests
	{
		private class TestCommand : Command
		{
			public TestCommand(ICommandMetadata _metadata) : base(_metadata) { }
			public override string GetAggregateRootId() => throw new NotImplementedException();
			public override string GetRegionId() => throw new NotImplementedException();
		}

		[Fact]
		public void construct_with_metadata()
		{
			var md = CommandMetadata.Empty;
			var command = new TestCommand(md);
			Assert.Equal(md, command._Metadata);
		}
	}
}
