using AnjUx.Server.Services;
using AnjUx.Shared.Models.Data;
using Microsoft.AspNetCore.Mvc;

namespace AnjUx.Server.Controllers
{
    [ApiController]
    public class TarefaController : BaseController<TarefaService, Tarefa>
    {
        [HttpGet("Limpar")]
        public async Task<ActionResult> Limpar()
        {
            await Service.Limpar();

            return Sucesso(true);
        }
    }
}
