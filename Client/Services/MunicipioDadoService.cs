using AnjUx.Shared.Models.Data;

namespace AnjUx.Client.Services
{
	public class MunicipioDadoService(HttpClient http, CoreNotificationService notificationService, LoadingService loadingService) : BaseService<MunicipioDado>(http, notificationService, loadingService)
	{
		protected override string BaseUrl => "api/municipioDado";

		public async Task<List<MunicipioDado>?> ListarPorMunicipio(long? municipioId)
			=> await MakeRequestAsync<List<MunicipioDado>>(HttpMethod.Get, $"ListarPorMunicipio/{municipioId}");
	}
}
