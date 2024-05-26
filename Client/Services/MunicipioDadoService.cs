using AnjUx.Shared.Models.Data;
using AnjUx.Shared.Models.Requests;
using AnjUx.Shared.Models.Response;

namespace AnjUx.Client.Services
{
	public class MunicipioDadoService(HttpClient http, CoreNotificationService notificationService, LoadingService loadingService) : BaseService<MunicipioDado>(http, notificationService, loadingService)
	{
		protected override string BaseUrl => "api/municipioDado";

		public async Task<List<MunicipioDado>?> ListarPorMunicipio(long? municipioId)
			=> await MakeRequestAsync<List<MunicipioDado>>(HttpMethod.Get, $"ListarPorMunicipio/{municipioId}");

		public async Task<ComparadorModel?> Comparador(RequestComparador request)
			=> await MakeRequestAsync<ComparadorModel, RequestComparador>(HttpMethod.Post, "Comparador", request);
	}
}
