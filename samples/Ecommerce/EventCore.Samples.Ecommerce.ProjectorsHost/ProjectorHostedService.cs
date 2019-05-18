using EventCore.Projectors;
using EventCore.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.ProjectorsHost
{
	public class ProjectorHostedService<TProjector> : BackgroundService where TProjector : IProjector
	{
		private readonly ILogger _logger;

		public ProjectorHostedService(ILogger logger)
		{
			_logger = logger;
		}


		protected async override Task ExecuteAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Projector hosted service starting: " + typeof(TProjector).Name);

			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					await cancellationToken.WaitHandle.AsTask();
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Exception in projector hosted service: " + typeof(TProjector).Name);
				}
			}

			_logger.LogInformation("Projector hosted service stopping: " + typeof(TProjector).Name);
		}
	}
}
