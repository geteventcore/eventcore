using System;
using Xunit;

namespace EventCore.AggregateRoots.Tests
{
	public class CommandTests
	{
		private class TestCommand : Command
		{
			public override string GetCommandId() => throw new NotImplementedException();
			public override string GetAggregateRootId() => throw new NotImplementedException();
			public override string GetRegionId() => throw new NotImplementedException();
		}

		[Fact]
		public void validate_semantics()
		{
			var command = new TestCommand();
			var result = command.ValidateSemantics();
			Assert.True(result.IsValid);
		}
	}
}
