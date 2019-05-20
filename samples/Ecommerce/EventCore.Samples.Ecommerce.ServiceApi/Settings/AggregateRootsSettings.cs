using Microsoft.Extensions.Configuration;

namespace EventCore.Samples.Ecommerce.ServiceApi.Settings
{
	public class AggregateRootsSettings
	{
		public static AggregateRootsSettings Get(IConfiguration config) => config.GetSection("Services:AggregateRoots").Get<AggregateRootsSettings>();
		
		public string AggregateRootStateBasePath { get; set; }
	}
}

