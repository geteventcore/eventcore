using Microsoft.Extensions.Configuration;

namespace EventCore.Samples.Ecommerce.ServiceApi.Settings
{
	public class EventSourcingSettings
	{
		public static EventSourcingSettings Get(IConfiguration config) => config.GetSection("Services:EventSourcing").Get<EventSourcingSettings>();

		public int StreamReadBatchSize { get; set; }
		public int ReconnectDelaySeconds { get; set; }
	}
}

