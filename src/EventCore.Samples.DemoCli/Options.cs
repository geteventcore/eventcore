using CommandLine;

namespace EventCore.Samples.DemoCli
{
	public static class Options
	{
		[Verb("commit", HelpText = "Commits events to demo streams.")]
		public class CommitOptions
		{
		}

		[Verb("load", HelpText = "Loads events from demo streams.")]
		public class LoadOptions
		{
		}
	}
}
