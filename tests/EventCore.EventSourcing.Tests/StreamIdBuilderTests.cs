using System;
using Xunit;

namespace EventCore.EventSourcing.Tests
{
	public class StreamIdBuilderTests
	{
		[Fact]
		public void throw_when_aggregate_root_name_not_provided()
		{
			var builder = new StreamIdBuilder();
			Assert.Throws<ArgumentNullException>(() => builder.Build("region", "context", null, "1"));
			Assert.Throws<ArgumentNullException>(() => builder.Build("region", "context", "", "1"));
		}

		[Fact]
		public void build_stream_id_from_only_aggregate_root_name()
		{
			var builder = new StreamIdBuilder();
			var aggregateRootName = "testAr";
			var streamId = builder.Build(null, null, aggregateRootName, null);
			Assert.Equal(streamId, aggregateRootName, StringComparer.OrdinalIgnoreCase);
		}

		[Fact]
		public void build_stream_id_from_all_fragments()
		{
			var builder = new StreamIdBuilder();
			var regionId = "region";
			var context = "context";
			var aggregateRootName = "testAr";
			var aggregateRootId = "1";
			var expectedStreamId = string.Join(StreamIdBuilder.SEPARATOR, regionId, context, aggregateRootName, aggregateRootId);
			var actualStreamId = builder.Build(regionId, context, aggregateRootName, aggregateRootId);

			Assert.Equal(expectedStreamId, actualStreamId, StringComparer.OrdinalIgnoreCase);
		}
	}
}
