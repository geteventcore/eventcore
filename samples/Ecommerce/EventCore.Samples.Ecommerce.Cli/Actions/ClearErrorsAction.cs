﻿using EventCore.StatefulSubscriber;
using EventCore.Utilities;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Cli.Actions
{
	public class ClearErrorsAction : IAction
	{
		private readonly Options.ClearErrorsOptions _options;
		private readonly IStandardLogger _logger;
		private readonly IConfiguration _config;

		public ClearErrorsAction(Options.ClearErrorsOptions options, IStandardLogger logger, IConfiguration config)
		{
			_options = options;
			_logger = logger;
			_config = config;
		}

		public async Task RunAsync()
		{
			Console.WriteLine("Clearing errors for all subscriber stream states.");

			// StatefulSubscriber keeps track of streams for which handlers have thrown errors.
			// When a handler throws an unhandled exception, no further events will be handled on the stream
			// until the error state is cleared. Note that this doesn't reset position information for each stream,
			// so processing/handling will pick back up at the position of the last error.
			// This is done to allow the subscriber to continue handling events on non-errored streams while requiring
			// an explicit reset of error state at the discretion of developers - e.g. after deploying a fix to a
			// handler where a bug caused the unhandled exception.

			// Clear stream state errors for projectors.
			await ClearStreamStateAsync(_config.GetValue<string>("StreamStatePaths:EmailReportProjector"));
			await ClearStreamStateAsync(_config.GetValue<string>("StreamStatePaths:SalesReportProjector"));

			// Clear stream state errors for process managers.
			// ...

			// Clear stream state errors for integrators.
			// ...

			Console.WriteLine("Done clearing errors.");
		}

		public Task ClearStreamStateAsync(string basePath) => new StreamStateRepo(_logger, basePath).ClearStreamStateErrorsAsync(CancellationToken.None);
	}
}
