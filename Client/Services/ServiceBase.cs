using static System.Net.WebRequestMethods;
using System.Net.Http.Json;
using Radzen;
using AnjUx.Client.Extensions;

namespace AnjUx.Client.Services
{
    public abstract class ServiceBase : IServiceBase
    {
        protected readonly HttpClient http;
        private readonly CoreNotificationService notificationService;
        private readonly LoadingService loadingService;

        public ServiceBase(HttpClient http, CoreNotificationService notificationService, LoadingService loadingService)
        {
            this.http = http;
            this.notificationService = notificationService;
            this.loadingService = loadingService;
        }

        protected async Task<ServiceResponse<TResponse>> HttpCallAsync<TResponse>(HttpMethod method, string requestUri)
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

        protected async Task<ServiceResponse<TResponse>> HttpCallAsync<TResponse, TBody>(HttpMethod method, string requestUri, TBody? obj)
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

        protected T ThreatServiceResponse<T>(ServiceResponse<T> response)
        {
            try
            {
                return InternalThreatServiceResponse<T>(response);
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
                return await httpresponse.Content.ReadFromJsonAsync<ServiceResponse<TResponse>>();
            }
            else
            {
                var response = await httpresponse.Content.ReadFromJsonAsync<ServiceResponse<string>>();
                notificationService.NotifyFail(response.Data);
                return new ServiceResponse<TResponse> { Success = false, Message = response.Message, Data = default };
            }
        }

        private T InternalThreatServiceResponse<T>(ServiceResponse<T> response)
        {
            if (response != null)
            {
                if (response.Success)
                {
                    return response.Data;
                }
                else
                {
                    notificationService.NotifyFail(response.Message, null, response);
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
