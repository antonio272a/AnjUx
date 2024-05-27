using AnjUx.Shared.Models.Data;

namespace AnjUx.Client.Services
{
    public class MunicipioService(HttpClient http, CoreNotificationService notificationService, LoadingService loadingService) : BaseService<Municipio>(http, notificationService, loadingService)
    {
        protected override string BaseUrl => "api/municipio";

        public async Task<bool> AtualizarMunicipios()
            => await MakeRequestAsync<bool>(HttpMethod.Get, "AtualizarMunicipios", notify: false, disableLoading: true);

        public async Task<List<Municipio>?> Buscar(string? termo)
            => await MakeRequestAsync<List<Municipio>>(HttpMethod.Get, $"Buscar?termo={termo}");
    
        public async Task<bool> BuscarReceitas(long? id, int? ano = null, int? mes = null)
            => await MakeRequestAsync<bool>(HttpMethod.Get, $"BuscarReceitas/{id}?ano={ano}&mes={mes}", notify: false, disableLoading: true);

        public async Task<bool> AtualizarTodasReceitas()
            => await MakeRequestAsync<bool>(HttpMethod.Get, "AtualizarTodasReceitas", notify: false, disableLoading: true);

        public async Task<bool> AtualizarTodasEstimativasPopulacionais()
            => await MakeRequestAsync<bool>(HttpMethod.Get, "AtualizarTodasEstimativasPopulacionais", notify: false, disableLoading: true);
    }
}
