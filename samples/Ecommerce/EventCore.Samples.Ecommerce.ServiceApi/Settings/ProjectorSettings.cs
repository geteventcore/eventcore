using Microsoft.Extensions.Configuration;

namespace EventCore.Samples.Ecommerce.ServiceApi.Settings
{
	public class ProjectorSettings
	{
		public static ProjectorSettings Get(IConfiguration config, string name) => config.GetSection($"Services:Projectors:{name}").Get<ProjectorSettings>();

		public string StreamStateBasePath { get; set; }
	}
}

