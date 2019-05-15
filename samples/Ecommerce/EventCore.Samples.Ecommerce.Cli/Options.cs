using CommandLine;

namespace EventCore.Samples.Ecommerce.Cli
{
	public static class Options
	{
		[Verb("initialize", HelpText = "Configure infrastructure before running apps. Expects infrastructure to be running.")]
		public class InitializeOptions
		{
		}

		[Verb("listen", HelpText = "Listen for and print all business events as they are committed.")]
		public class ListenOptions
		{
			[Value(0, MetaName = "verbose", Required = false, Default = false, HelpText = "True to decode and show event payloads.")]
			public bool Verbose { get; set; }
		}
	}
}
