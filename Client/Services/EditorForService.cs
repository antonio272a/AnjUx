using Radzen;

namespace AnjUx.Client.Services
{
    public class EditorForService : ServiceBase, IServiceBase
    {
        public EditorForService(HttpClient http, CoreNotificationService notificationService, LoadingService loadingService) : base(http, notificationService, loadingService) { }

        public async Task<List<T>> Search<T>(string requestUri)
        {
            var response = await HttpCallAsync<List<T>>(HttpMethod.Get, requestUri);
            return ThreatServiceResponse(response);
        }
    }
}
