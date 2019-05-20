using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EventCore.Samples.Ecommerce.ServiceApi
{
	public class Startup
	{
		public IConfiguration Configuration { get; }

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1",
					new Swashbuckle.AspNetCore.Swagger.Info
					{
						Title = "Ecommerce Sample - Service API",
						Version = "v1",
						Description = "API and host for backend services, including: aggregate roots, projectors, and  process managers."
					});
			});

			StartupSupport.ServiceConfiguration.Configure(Configuration, services);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceScopeFactory scopeFactory, IServiceProvider serviceProvider)
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

			// Force event store connections to start.
			serviceProvider.GetRequiredService<EventSourcing.IStreamClientFactory>();
		}
	}
}
