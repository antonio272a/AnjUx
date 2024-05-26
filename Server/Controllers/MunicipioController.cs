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

        [HttpGet("BuscarInformacoes/{id}")]
        public async Task<ActionResult> BuscarInformacoes(long? id)
        {
            await Service.BuscarInformacoes(id);

            return Sucesso(true);
        }

        [HttpGet("listar")]
        public override async Task<ActionResult> Listar()
        {
            List<Municipio> municipios = await Service.Listar(100);

            return Sucesso(municipios);
        }
    }
}
