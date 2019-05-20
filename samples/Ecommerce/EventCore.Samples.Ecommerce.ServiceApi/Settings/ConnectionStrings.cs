using Microsoft.Extensions.Configuration;

namespace EventCore.Samples.Ecommerce.ServiceApi.Settings
{
	public class ConnectionStrings
	{
		private readonly IConfiguration _config;

		public ConnectionStrings(IConfiguration config)
		{
			_config = config;
		}

		public static ConnectionStrings Get(IConfiguration config) => new ConnectionStrings(config);

		public string EventStoreRegionX { get => _config.GetConnectionString("EventStoreRegionX"); }
		public string AggRootStatesDb { get => _config.GetConnectionString("AggRootStatesDb"); }
		public string ProjectionsDb { get => _config.GetConnectionString("ProjectionsDb"); }
	}
}

