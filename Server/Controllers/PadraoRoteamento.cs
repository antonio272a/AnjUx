using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace AnjUx.Server.Controllers
{
    public class PadraoRoteamento: IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            // Essa classe é responsável por adicionar o attributo de Rota para os controllers
            // Utilizando o namespace para definir a área
            string area = controller.ControllerType.Namespace?.Split('.').Last()!;

            string route;

            if(area == "Controllers")
                route = "api/[controller]";
            else
                route = $"api/{area}/[controller]";


            controller.Selectors[0].AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(route));
        }
    }
}
