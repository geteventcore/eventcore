using Xunit;

namespace EventCore.EventSourcing.Tests
{
	public class BusinessEventMetadataTests
	{
		[Fact]
		public void construct()
		{
			var causalId = "ca";
			var correlationId = "cor";

			var metadata = new BusinessEventMetadata(causalId, correlationId);

			Assert.Equal(causalId, metadata.CausalId);
			Assert.Equal(correlationId, metadata.CorrelationId);
		}
	}
}
