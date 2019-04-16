using System;
using Xunit;

namespace EventCore.EventSourcing.Tests
{
	public class StreamIdBuilderTests
	{
		[Fact]
		public void validate_valid_stream_chars()
		{
			var validChars = "AaZz09_-";
			Assert.True(StreamIdBuilder.ValidateStreamIdChars(validChars));
		}

		[Fact]
		public void not_validate_invalid_stream_chars()
		{
			var invalidChars = "!";
			Assert.False(StreamIdBuilder.ValidateStreamIdChars(invalidChars));
		}

		[Fact]
		public void throw_when_aggregate_root_name_not_provided()
		{
			var builder = new StreamIdBuilder();
			Assert.Throws<ArgumentNullException>(() => builder.Build("region", "context", null, "1"));
			Assert.Throws<ArgumentNullException>(() => builder.Build("region", "context", "", "1"));
		}

		[Fact]
		public void throw_when_invalid_chars_in_region_id()
		{
			var builder = new StreamIdBuilder();
			Assert.Throws<ArgumentException>(() => builder.Build("region!", "context", "agg", "1"));
		}

		[Fact]
		public void throw_when_invalid_chars_in_context()
		{
			var builder = new StreamIdBuilder();
			Assert.Throws<ArgumentException>(() => builder.Build("region", "context!", "agg", "1"));
		}

		[Fact]
		public void throw_when_invalid_chars_in_aggregate_root_name()
		{
			var builder = new StreamIdBuilder();
			Assert.Throws<ArgumentException>(() => builder.Build("region", "context", "agg!", "1"));
		}
		[Fact]
		public void throw_when_invalid_chars_in_aggregate_root_id()
		{
			var builder = new StreamIdBuilder();
			Assert.Throws<ArgumentException>(() => builder.Build("region!", "context", "agg", "!"));
		}

		[Fact]
		public void build_stream_id_from_only_aggregate_root_name()
		{
			var builder = new StreamIdBuilder();
			var aggregateRootName = "testAr";
			var streamId = builder.Build(null, null, aggregateRootName, null);
			Assert.Equal(streamId, aggregateRootName);
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

			Assert.Equal(expectedStreamId, actualStreamId);
		}
	}
}
