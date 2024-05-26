using AnjUx.Shared.Models.Data;
using AnjUx.Shared.Models.Enums;

namespace AnjUx.Client.Services
{
	public class MunicipioDadoService(HttpClient http, CoreNotificationService notificationService, LoadingService loadingService) : BaseService<MunicipioDado>(http, notificationService, loadingService)
	{
		protected override string BaseUrl => "api/municipioDado";

		public async Task<List<MunicipioDado>?> ListarPorMunicipio(long? municipioId, TipoDado? tipo = null, int? ano = null, Mes? mes = null)
			=> await MakeRequestAsync<List<MunicipioDado>>(HttpMethod.Get, $"ListarPorMunicipio/{municipioId}?tipo={tipo}&ano={ano}&mes={mes}");
	}
}
