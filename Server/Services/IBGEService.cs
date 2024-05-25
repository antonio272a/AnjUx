using AnjUx.Shared.Models.Data;
using System.Text.Json;

namespace AnjUx.Server.Services
{
    public class IBGEService
    {
        private readonly string baseUrl = "https://servicodados.ibge.gov.br/api/";

        public async Task<List<Municipio>> GetMunicipios()
        {
            using HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync($"{baseUrl}v1/localidades/municipios");

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Municipio>>(content)!;
            }

            string erro = await response.Content.ReadAsStringAsync();

            throw new Exception($"Erro ao buscar municípios: {erro}");
        }
    }
}
