using AnjUx.Server.Services;
using AnjUx.Shared.Models.Data;
using Microsoft.AspNetCore.Mvc;

namespace AnjUx.Server.Controllers
{
    [ApiController]
    public class TarefaController : BaseController<TarefaService, Tarefa>
    {
    }
}
