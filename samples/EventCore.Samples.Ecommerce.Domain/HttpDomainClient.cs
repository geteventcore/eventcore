using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.Domain
{
	public abstract class HttpDomainClient
	{
		private readonly HttpClient _httpClient;
		private readonly string _baseUrl;

		public HttpDomainClient(string baseUrl, HttpClient httpClient)
		{
			_httpClient = httpClient;
			_baseUrl = baseUrl.TrimEnd('/');
		}

		private string BuildPostUrl(string aggregateRootName, string commandName) => string.Join("/", _baseUrl, aggregateRootName, commandName);

		public Task PostCommandAsync<TCommand>(TCommand command) where TCommand : DomainCommand
			=> PostCommandAsync(BuildPostUrl(command._AggregateRootName, command._CommandName), command);

		private async Task PostCommandAsync(string postUrl, DomainCommand command)
		{
			var response = await _httpClient.PostAsync(postUrl, new StringContent(JsonConvert.SerializeObject(command), Encoding.Unicode, "application/json"));

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