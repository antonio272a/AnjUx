using AnjUx.Shared.Models.Response;
using AnjUx.Shared.Tools;

namespace AnjUx.Server.Services
{
    public class IBGEService
    {
        private readonly string baseUrl = "https://servicodados.ibge.gov.br/api/";

        public async Task<List<MunicipioIBGE>> GetMunicipios()
            => await new HttpClientWrapper(baseUrl).Get<List<MunicipioIBGE>>("v1/localidades/municipios");
    }
}
