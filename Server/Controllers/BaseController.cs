using AnjUx.Client.Services;
using AnjUx.Shared.Extensions;
using AnjUx.Shared.Interfaces;
using AnjUx.Server.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AnjUx.Server.Controllers
{
    public abstract class BaseController<S, M> : ControllerBase, IDisposable, IBaseController
        where S : IBaseService<M>
        where M : IDbModel
    {
        private string? nomeUsuario;
        protected S Service { get; set; }

        public string? NomeUsuario
        {
            get => nomeUsuario;
            set {
                nomeUsuario = value;
                Service.NomeUsuario = value;
            } 
        }

        public BaseController()
        {
            Service = BuildService();
        }

        protected virtual S BuildService() => (S)Activator.CreateInstance(typeof(S), [null, null])!;

        protected ActionResult Sucesso<T>(T data)
        {
            return Ok(new ServiceResponse<T> { Success = true, Data = data });
        }

        protected ActionResult NaoEncontrado(string message)
        {
            return NotFound(new ServiceResponse<string> { Success = false, Message = message });
        }

        [HttpGet("{id}")]
        public virtual ActionResult Get(long? id)
        {
            M? objeto = Service.GetByID(id);

            if (!objeto.IsPersisted())
                return NaoEncontrado($"\"{typeof(M).Name}\" não encontrado!");
            else
                return Sucesso(objeto);
        }

        [HttpGet("listar")]
        public virtual ActionResult Listar()
        {
            List<M> objetos = Service.Listar();

            return Sucesso(objetos);
        }

        [HttpPost("")]
        public virtual ActionResult Criar([FromBody] M model)
        {
            Service.Save(model);

            return Sucesso(model);
        }

        [HttpPut("")]
        public virtual ActionResult Editar([FromBody] M model)
        {
            bool response = Service.Save(model);

            return Sucesso(response);
        }

        [HttpDelete("{id}")]
        public virtual ActionResult Excluir(long? id)
        {
            bool response = Service.Delete(id);

            return Sucesso(response);
        }

        public void Dispose()
        {
            Service.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
