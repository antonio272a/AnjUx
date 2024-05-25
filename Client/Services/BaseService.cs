using AnjUx.Client.Extensions;
using AnjUx.Client.Services;
using AnjUx.Shared.Interfaces;
using System.Net.Http.Json;

namespace AnjUx.Client.Services
{
    public abstract class BaseService<T>(HttpClient http, CoreNotificationService notificationService, LoadingService loadingService) : BaseService(http, notificationService, loadingService)
        where T : IDbModel
    {
        public virtual async Task<List<T>?> Listar() => await MakeRequestAsync<List<T>>(HttpMethod.Get, $"listar");

        public virtual async Task<T?> Criar(T objeto) => await MakeRequestAsync<T, T>(HttpMethod.Post, "", objeto);

        public virtual async Task<bool> Editar(T objeto) => await MakeRequestAsync<bool, T>(HttpMethod.Put, "", objeto);

        public virtual async Task<T?> GetByID(long? id) => await MakeRequestAsync<T>(HttpMethod.Get, $"{id}");

        public virtual async Task<bool> Excluir(long? id) => await MakeRequestAsync<bool>(HttpMethod.Delete, $"{id}");
    }

    public abstract class BaseService(HttpClient http, CoreNotificationService notificationService, LoadingService loadingService) : IBaseService
    {
        protected abstract string BaseUrl { get; }

        protected readonly HttpClient http = http;
        private readonly CoreNotificationService notificationService = notificationService;
        private readonly LoadingService loadingService = loadingService;

        protected async Task<TResponse?> MakeRequestAsync<TResponse>(HttpMethod method, string requestUri, bool notify = true)
        {
            ServiceResponse<TResponse> response = await HttpCallAsync<TResponse>(method, BuildUrl(requestUri));

            return ThreatServiceResponse(response, notify);
        }

        protected async Task<TResponse?> MakeRequestAsync<TResponse, TBody>(HttpMethod method, string requestUri, TBody? obj)
        {
            ServiceResponse<TResponse> response = await HttpCallAsync<TResponse, TBody>(method, BuildUrl(requestUri), obj);

            return ThreatServiceResponse(response);
        }

        private string BuildUrl(string requestUri)
        {
            string url = BaseUrl;

            if (!url.EndsWith('/'))
                url += "/";

            return $"{url}{requestUri}";
        }

        private async Task<ServiceResponse<TResponse>> HttpCallAsync<TResponse>(HttpMethod method, string requestUri)
        {
            try
            {
                loadingService.IsLoading = true;
                return await InternalHttpCallAsync<TResponse, object>(method, requestUri, default);
            }
            catch (Exception ex)
            {
                notificationService.NotifyException(ex);
                return new ServiceResponse<TResponse> { Success = false, Message = ex.ToString(), Data = default };
            }
            finally
            {
                loadingService.IsLoading = false;
            }
        }

        private async Task<ServiceResponse<TResponse>> HttpCallAsync<TResponse, TBody>(HttpMethod method, string requestUri, TBody? obj)
        {
            try
            {
                loadingService.IsLoading = true;
                return await InternalHttpCallAsync<TResponse, TBody>(method, requestUri, obj);
            }
            catch (Exception ex)
            {
                notificationService.NotifyException(ex);
                return new ServiceResponse<TResponse> { Success = false, Message = ex.ToString(), Data = default };
            }
            finally
            {
                loadingService.IsLoading = false;
            }
        }

        private T? ThreatServiceResponse<T>(ServiceResponse<T> response, bool notify = true)
        {
            try
            {
                return InternalThreatServiceResponse<T>(response, notify);
            }
            catch (Exception ex)
            {
                notificationService.NotifyException(ex);
                return default;
            }
        }

        private async Task<ServiceResponse<TResponse>> InternalHttpCallAsync<TResponse, TBody>(HttpMethod method, string requestUri, TBody? obj)
        {
            HttpResponseMessage httpresponse;

            switch (method.Method)
            {
                case "GET":
                    httpresponse = await http.GetAsync(requestUri);
                    break;

                case "POST":
                    httpresponse = await http.PostAsJsonAsync(requestUri, obj);
                    break;

                case "PUT":
                    httpresponse = await http.PutAsJsonAsync(requestUri, obj);
                    break;

                case "DELETE":
                    httpresponse = await http.DeleteAsync(requestUri);
                    break;

                default:
                    throw new ArgumentException($"Http Method {method.Method} not mapped to be Threated.");
            }

            // Verifica se ocorreu tudo certo e retorna
            if (httpresponse.IsSuccessStatusCode)
            {
                return (await httpresponse.Content.ReadFromJsonAsync<ServiceResponse<TResponse>>())!;
            }
            else if (httpresponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var response = await httpresponse.Content.ReadFromJsonAsync<ServiceResponse<string>>();
                return new ServiceResponse<TResponse> { Success = false, Message = response?.Message ?? "Usuário Não Autorizado!", Data = default };
            }
            else
            {
                var response = await httpresponse.Content.ReadFromJsonAsync<ServiceResponse<string>>();
                return new ServiceResponse<TResponse> { Success = false, Message = response!.Message, Data = default };
            }
        }

        private T? InternalThreatServiceResponse<T>(ServiceResponse<T> response, bool notify = true)
        {
            if (response != null)
            {
                if (response.Success)
                {
                    return response!.Data!;
                }
                else
                {
                    if (notify) notificationService.NotifyFail(response.Message, null!, response);
                    return default;
                }
            }
            else
            {
                throw new Exception("ServiceResponse was null");
            }
        }
    }
}
