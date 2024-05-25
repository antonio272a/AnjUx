using AnjUx.Shared.Models.Data;
using AnjUx.Shared.Models.Response;
using System.Text.Json;

namespace AnjUx.Server.Services
{
    public class IBGEService
    {
        private readonly string baseUrl = "https://servicodados.ibge.gov.br/api/";

        public async Task<List<MunicipioIBGE>> GetMunicipios()
        {
            using HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync($"{baseUrl}v1/localidades/municipios");

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<MunicipioIBGE>>(content)!;
            }

            string erro = await response.Content.ReadAsStringAsync();

            throw new Exception($"Erro ao buscar municípios: {erro}");
        }
    }
}
