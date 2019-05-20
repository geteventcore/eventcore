using EventCore.Projectors;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.ServiceApi.Infrastructure
{
	public class ProjectorHostedService<TProjector> : BackgroundService where TProjector : IProjector
	{
		private readonly ILogger _logger;
		private readonly TProjector _projector;

		public ProjectorHostedService(ILogger<ProjectorHostedService<TProjector>> logger, TProjector projector)
		{
			_logger = logger;
			_projector = projector;
		}

		protected async override Task ExecuteAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Projector hosted service starting: " + typeof(TProjector).Name);

			try
			{
				await _projector.RunAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception in projector hosted service: " + typeof(TProjector).Name);
			}

			_logger.LogInformation("Projector hosted service stopping: " + typeof(TProjector).Name);
		}
	}
}
