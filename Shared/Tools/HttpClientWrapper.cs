using Newtonsoft.Json;

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
				string content = await response.Content.ReadAsStringAsync();
				return JsonConvert.DeserializeObject<T>(content)!;
			}

			string erro = await response.Content.ReadAsStringAsync();

			throw new Exception($"Erro ao buscar {resource}: {erro}");
		}
	}
}
