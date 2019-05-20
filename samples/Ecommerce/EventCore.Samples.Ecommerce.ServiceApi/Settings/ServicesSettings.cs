using Microsoft.Extensions.Configuration;

namespace EventCore.Samples.Ecommerce.ServiceApi.Settings
{
	public class ServicesSettings
	{
		public static ServicesSettings Get(IConfiguration config) => config.GetSection("Services").Get<ServicesSettings>();

		public AggregateRootsSettings AggregateRoots { get; set; }
		public ProjectorsSettings Projectors { get; set; }
	}
}

