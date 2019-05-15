using System;
using Xunit;

namespace EventCore.AggregateRoots.Tests
{
	public class CommandTests
	{
		private class TestCommand : Command
		{
			public TestCommand(CommandMetadata _metadata) : base(_metadata) {}
			public override string GetAggregateRootId() => throw new NotImplementedException();
			public override string GetRegionId() => throw new NotImplementedException();
		}

		[Fact]
		public void validate_semantics()
		{
			var command = new TestCommand(CommandMetadata.Default);
			var result = command.ValidateSemantics();
			Assert.True(result.IsValid); // TODO: Implement validation attributes.
		}
	}
}
