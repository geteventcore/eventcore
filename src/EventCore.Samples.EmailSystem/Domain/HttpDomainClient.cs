using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.Domain
{
	public abstract class HttpDomainClient
	{
		private readonly string _baseUrl;

		public HttpDomainClient(string baseUrl)
		{

			_baseUrl = baseUrl.TrimEnd('/') + '/';
		}

		private string BuildPostUrl(string path) => _baseUrl + path.TrimStart('/');

		protected async Task PostCommandAsync(string commandUrlPath, DomainCommand command)
		{
			var postUrl = BuildPostUrl(commandUrlPath);

			using (var client = new HttpClient())
			{
				var response = await client.PostAsync(postUrl, new StringContent(JsonConvert.SerializeObject(command), Encoding.Unicode, "application/json"));

				switch (response.StatusCode)
				{
					case HttpStatusCode.OK:
						return;
					case HttpStatusCode.BadRequest:
						throw new ArgumentException(await response.Content.ReadAsStringAsync());
					default:
						throw new ApplicationException($"Unexpected server response: ({response.StatusCode}) {response.Content.ReadAsStringAsync().Result}");
				}
			}
		}
	}
}