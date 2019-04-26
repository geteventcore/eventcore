using CommandLine;

namespace EventCore.Samples.DemoCli
{
	public static class Options
	{
		[Verb("commit", HelpText = "Commits events to demo streams.")]
		public class CommitOptions
		{
			[Value(0, MetaName = "streamsPerAgg", HelpText = "Number of streams per aggregate type.")]
			public int? StreamsPerAgg { get; set; } = 10;

			[Value(1, MetaName = "eventGroupsPerStream", HelpText = "Number of event groups per stream.")]
			public int? EventGroupsPerStream { get; set; } = 10;
		}

		[Verb("load", HelpText = "Loads events from demo streams.")]
		public class LoadOptions
		{
			[Value(0, MetaName = "fromPosition", HelpText = "Starting position to load events.")]
			public int FromPosition { get; set; } = 0;
		}

		[Verb("subscribe", HelpText = "Subscribes to events from demo streams.")]
		public class SubscribeOptions
		{
			[Value(0, MetaName = "fromPosition", HelpText = "Starting position to subscribe events.")]
			public int FromPosition { get; set; } = 0;
		}
	}
}
