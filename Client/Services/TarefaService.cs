using AnjUx.Shared.Models.Data;

namespace AnjUx.Client.Services
{
    public class TarefaService(HttpClient http, CoreNotificationService notificationService, LoadingService loadingService) : BaseService<Tarefa>(http, notificationService, loadingService)
    {
        protected override string BaseUrl => "api/tarefa";

        public async Task<bool> Limpar()
            => await MakeRequestAsync<bool>(HttpMethod.Get, "Limpar");
    }
}
