using AnjUx.Server.Services;
using AnjUx.Shared.Models.Data;
using AnjUx.Shared.Models.Requests;
using AnjUx.Shared.Models.Response;
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

		[HttpPost("Comparador")]
		public async Task<ActionResult> Comparador([FromBody] RequestComparador request)
        {
            ComparadorModel response = await Service.Comparador(request);

            return Sucesso(response);
        }
	}
}
