using Newtonsoft.Json;
using System.Net.Http.Json;

namespace AnjUx.Shared.Tools
{
	public class HttpClientWrapper
	{
        private HttpClient HttpClient { get; set; }

		public HttpClientWrapper(string baseUrl)
		{
			HttpClient = new HttpClient();
			HttpClient.BaseAddress = new Uri(baseUrl);
		}

		public async Task<T> Get<T>(string resource)
		{
			HttpResponseMessage response = await HttpClient.GetAsync(resource);

			if (response.IsSuccessStatusCode)
			{
				string content = response.Content.ReadAsStringAsync().Result;
				return JsonConvert.DeserializeObject<T>(content)!;
			}

			string erro = response.Content.ReadAsStringAsync().Result;

			throw new Exception($"Erro ao buscar {resource}: {erro}");
		}

		public async Task<R> Post<T, R>(string? resource, T? body)
		{
			HttpResponseMessage response = await HttpClient.PostAsJsonAsync(resource, body);

			if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<R>(content)!;
            }

			string erro = await response.Content.ReadAsStringAsync();
			throw new Exception($"Erro ao buscar {resource}: {erro}");
		} 
	}
}
