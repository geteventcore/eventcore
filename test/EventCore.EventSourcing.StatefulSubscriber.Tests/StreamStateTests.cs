using System.Text;
using Xunit;

namespace EventCore.EventSourcing.StatefulSubscriber.Tests
{
	public class StreamStateTests
	{
		[Fact]
		public void construct()
		{
			var lastProcessedPosition = (long)1;
			var hasError = true;

			var state = new StreamState(lastProcessedPosition, hasError);

			Assert.Equal(lastProcessedPosition, state.LastAttemptedPosition);
			Assert.Equal(hasError, state.HasError);
		}
	}
}
