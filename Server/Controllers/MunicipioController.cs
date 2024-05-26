using AnjUx.Server.Services;
using AnjUx.Shared.Models.Data;
using Microsoft.AspNetCore.Mvc;

namespace AnjUx.Server.Controllers
{
    [ApiController]
    public class MunicipioController : BaseController<MunicipioService, Municipio>
    {
        [HttpGet("AtualizarMunicipios")]
        public async Task<ActionResult> AtualizarMunicipios()
        {
            await Service.AtualizarMunicipios();

            return Sucesso(true);
        }

        [HttpGet("Buscar")]
        public async Task<ActionResult> Buscar(string? termo)
        {
            List<Municipio>? municipios = await Service.Buscar(termo);

            return Sucesso(municipios);
        }
    }
}
