using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventCore.Samples.Ecommerce.DomainApi
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
				c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info { Title = "Ecommerce Sample - Domain API", Version = "v1" });
			});

			AppServiceConfiguration.ConfigureServices(Configuration, services);
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
				c.SwaggerEndpoint("/_openapi/v1/openapi.json", "Ecommerce Sample- Domain API v1");
			});

		}
	}
}
