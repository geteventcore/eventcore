using EventCore.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Cli.Actions
{
	public class InitializeAction : IAction
	{
		private readonly Options.InitializeOptions _options;
		private readonly IStandardLogger _logger;
		private readonly IConfiguration _config;

		public InitializeAction(Options.InitializeOptions options, IStandardLogger logger, IConfiguration config)
		{
			_options = options;
			_logger = logger;
			_config = config;
		}

		public async Task RunAsync()
		{
			Console.WriteLine("Initializing infrastructure... ");

			// Do not rethrow - allow other configurations to proceed.

			try
			{
				await CreateEventStoreProjections();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

			try
			{
				await EnsureDatabasesCreatedAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

			Console.WriteLine("Done initializing infrastructure.");
		}

		private async Task CreateEventStoreProjections()
		{
			var eventStoreAdminUri = _config.GetValue<string>("EventStoreAdminUriRegionX");
			var postProjectionUrl = new Uri(new Uri(eventStoreAdminUri), "projections/continuous?name=$all_non_system&type=js&enabled=true&emit=true&trackemittedstreams=false");

			using (var httpClient = new HttpClient())
			{
				var byteArray = new UTF8Encoding().GetBytes(_config.GetValue<string>("EventStoreAdminCredsRegionX"));
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

				var result = await httpClient.PostAsync(postProjectionUrl, new StringContent(PROJECTION___ALL_NON_SYSTEM_EVENTS));

				if (result.StatusCode != HttpStatusCode.Created)
				{
					throw new Exception($"Unexpected result code from event store: {result.StatusCode} {result.Content.ToString()}");
				}
			}

			Console.WriteLine("Event store projections created.");
		}

		private async Task EnsureDatabasesCreatedAsync()
		{
			// Aggregate root state database. Single database will share multiple contexts.
			var aggRootStatesDbConnStr = _config.GetConnectionString("AggRootStatesDb");
			await EnsureDatabaseCreatedAsync<Domain.EmailBuilder.StateModels.EmailBuilderDbContext>(aggRootStatesDbConnStr, o => new Domain.EmailBuilder.StateModels.EmailBuilderDbContext(o));
			// ... any other agg root state contexts.

			Console.WriteLine("Agg Root States DB created/configured.");

			// ***

			// Projections database. Single database will share multiple contexts.
			var projectionsDbConnStr = _config.GetConnectionString("ProjectionsDb");
			await EnsureDatabaseCreatedAsync<Projections.EmailReport.EmailReportDb.EmailReportDbContext>(projectionsDbConnStr, o => new Projections.EmailReport.EmailReportDb.EmailReportDbContext(o));
			await EnsureDatabaseCreatedAsync<Projections.SalesReport.SalesReportDb.SalesReportDbContext>(projectionsDbConnStr, o => new Projections.SalesReport.SalesReportDb.SalesReportDbContext(o));
			// ... any other projection contexts.

			Console.WriteLine("Projections DB created/configured.");
		}

		private async Task EnsureDatabaseCreatedAsync<TContext>(string connectionString, Func<DbContextOptions<TContext>, TContext> ctor) where TContext : DbContext
		{
			var context = ctor(new DbContextOptionsBuilder<TContext>().UseSqlServer(connectionString).Options);
			var creator = ((RelationalDatabaseCreator)context.Database.GetService<IDatabaseCreator>());
			if(!(await creator.ExistsAsync()))
			{
				await creator.CreateAsync();
			}
			await creator.CreateTablesAsync();
		}

		private const string PROJECTION___ALL_NON_SYSTEM_EVENTS =
			@"fromAll().
				when({
							$any : function(s,e) {
									if (e.eventType && !e.eventType.startsWith('$'))
											linkTo('$allNonSystemEvents', e);
				}});";
	}
}
