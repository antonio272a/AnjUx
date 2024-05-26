using AnjUx.Server.Services;
using AnjUx.Shared.Models.Data;
using AnjUx.Shared.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace AnjUx.Server.Controllers
{
	[ApiController]
	public class MunicipioDadoController : BaseController<MunicipioDadoService, MunicipioDado>
	{
		[HttpGet("ListarPorMunicipio/{idMunicipio}")]
		public async Task<ActionResult> ListarPorMunicipio(long? idMunicipio, TipoDado? tipo, int? ano, Mes? mes)
		{
			List<MunicipioDado>? municipioDados = await Service.ListarPorMunicipio(idMunicipio, tipo, ano, mes);

			return Sucesso(municipioDados);
		}
	}
}
