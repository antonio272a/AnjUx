using AnjUx.Server.Services;
using AnjUx.Shared.Extensions;
using AnjUx.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace AnjUx.Server.Controllers
{
    [ApiController]
    public class GenericSearchController : BaseController<GenericSearchService, BaseModel>
    {
        [HttpGet("")]
        public async Task<ActionResult> Search(string? filter, string? typeName)
        {
            if (typeName.IsNullOrWhiteSpace())
                throw new ArgumentException("Informe o Tipo!");

            var assembly = Assembly.Load("AnjUx.Shared");
            Type modelType = assembly.GetType(typeName, false) ?? throw new ArgumentException("Tipo não encontrado!");

            if (modelType.IsAssignableFrom(typeof(BaseModel)))
                throw new ArgumentException("Tipo inválido!");

            MethodInfo openMethod = typeof(GenericSearchService).GetMethod("GenericSearch", BindingFlags.Public | BindingFlags.Instance)!;
            MethodInfo genericOpenMethod = openMethod.MakeGenericMethod(modelType);

            // Invoke the method asynchronously
            var task = (Task)genericOpenMethod.Invoke(Service, new object[] { filter! })!;
            await task.ConfigureAwait(false);

            // Get the result from the task
            var resultProperty = task.GetType().GetProperty("Result");
            var resultado = resultProperty!.GetValue(task);

            return Sucesso(resultado);
        }
    }
}
