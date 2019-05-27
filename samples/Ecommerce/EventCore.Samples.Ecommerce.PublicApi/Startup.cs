using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventCore.Samples.Ecommerce.PublicApi
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1",
					new Swashbuckle.AspNetCore.Swagger.Info
					{
						Title = "Ecommerce Sample - Public API",
						Version = "v1",
						Description = "Public REST API that acts as a fascade to backend services."
					});
			});

			// Projections
			services.AddDbContext<Projections.EmailReport.EmailReportDb.EmailReportDbContext>(o => o.UseSqlServer(Configuration.GetConnectionString("ProjectionsDb")));
			services.AddDbContext<Projections.SalesReport.SalesReportDb.SalesReportDbContext>(o => o.UseSqlServer(Configuration.GetConnectionString("ProjectionsDb")));

			// Domain Clients
			services.AddScoped<HttpClient>();
			services.AddScoped<Domain.Clients.SalesOrderClient>(sp => new Domain.Clients.SalesOrderClient(Configuration.GetValue<string>("DomainApiBasePath"), sp.GetRequiredService<HttpClient>()));
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();

			app.UseMvc();

			// Enable middleware to serve generated Swagger as a JSON endpoint.
			app.UseSwagger(c =>
				c.RouteTemplate = "_openapi/{documentName}/openapi.json"
			);

			// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
			// specifying the Swagger JSON endpoint.
			app.UseSwaggerUI(c =>
			{
				c.DocumentTitle = "Ecommerce Sample API Docs";
				// c.RoutePrefix = "_openapi";
				c.RoutePrefix = string.Empty;
				c.SwaggerEndpoint("/_openapi/v1/openapi.json", "Ecommerce Sample - Service API v1");
			});
		}
	}
}
