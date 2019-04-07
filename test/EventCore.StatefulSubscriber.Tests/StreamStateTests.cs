using System.Text;
using Xunit;

namespace EventCore.StatefulSubscriber.Tests
{
	public class StreamStateTests
	{
		[Fact]
		public void construct()
		{
			var lastProcessedPosition = (long)1;
			var hasError = true;

			var state = new StreamState(lastProcessedPosition, hasError);

			Assert.Equal(lastProcessedPosition, state.LastProcessedPosition);
			Assert.Equal(hasError, state.HasError);
		}
	}
}
