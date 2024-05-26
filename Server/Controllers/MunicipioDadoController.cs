using AnjUx.Server.Services;
using AnjUx.Shared.Models.Data;
using Microsoft.AspNetCore.Mvc;

namespace AnjUx.Server.Controllers
{
	[ApiController]
	public class MunicipioDadoController : BaseController<MunicipioDadoService, MunicipioDado>
	{
		[HttpGet("ListarPorMunicipio/{idMunicipio}")]
		public async Task<ActionResult> ListarPorMunicipio(long? idMunicipio)
		{
			List<MunicipioDado>? municipioDados = await Service.ListarPorMunicipio(idMunicipio);

			return Sucesso(municipioDados);
		}
	}
}
