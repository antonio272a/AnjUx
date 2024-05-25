using AnjUx.Client.Attributes;
using Microsoft.AspNetCore.Components;
using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AnjUx.Client.Shared
{
    public partial class CoreRoutes
    {

        #region Singleton

        private static CoreRoutes? instance;

        public static CoreRoutes Instance
        {
            get
            {
                instance ??= new CoreRoutes();
                return instance;
            }
        }

        private CoreRoutes()
        {

        }

        #endregion

        private readonly Dictionary<string, Type> rotas = [];
        private readonly Dictionary<string, Type> componentes = [];
        public IReadOnlyDictionary<string, Type> Rotas => rotas;
        public IReadOnlyDictionary<string, Type> Componentes => componentes;
        public HashSet<string> Areas { get; private set; } = [];
        public HashSet<string> Contextos { get; private set; } = [];
        private string? PagesNamespace { get; set; }

        public string GetRoute(Type page)
        {
            if (!page.IsAssignableTo(typeof(ComponentBase)))
                throw new ArgumentException("O tipo informado deve ser um Componente");

            if (!page.FullName!.StartsWith(PagesNamespace!))
                throw new ArgumentException("O componente informado não é uma página!");

            return GetRouteInternal(page);
        }

        private string GetRouteInternal(Type page)
        {
            RouteAttribute? routeAttribute = page.GetCustomAttribute<RouteAttribute>();

            string baseRoute;

            if (routeAttribute != null)
                baseRoute = routeAttribute.Template;
            else
                baseRoute = page.FullName!.Replace(PagesNamespace!, "").Replace(".", "/");

            string route = baseRoute.ToLower().Replace("index", "");

            route = MyRegex().Replace(route, "*");

            if (route.EndsWith('/') && route != "/")
                route = route[..^1];

            return route;
        }

        public void Initialize(Assembly appAssembly, string pagesNamespace)
        {
            PagesNamespace = pagesNamespace;
            List<Type> pages = appAssembly.GetTypes().Where(t => t.IsSubclassOf(typeof(ComponentBase)) && t.Namespace!.StartsWith(pagesNamespace)).ToList();
            Console.WriteLine("Rodou Initialize");
            foreach (Type page in pages)
            {
                string route = GetRouteInternal(page);

                if (rotas.ContainsKey(route))
                    throw new DuplicateNameException($"Rota duplicada: \"{route}\"");

                if (page.GetCustomAttribute<PartialOnlyAttribute>() == null)
                    rotas.Add(route, page);

                componentes.Add(route, page);

                List<string> partesRota = [.. route.Split('/')];

                if (partesRota.Count > 2)
                    Areas.Add(partesRota[1]);

                if (partesRota.Count > 3)
                    Contextos.Add(partesRota[2]);
            }
        }

        [GeneratedRegex(@"\{[a-zA-Z0-9]+(:[a-zA-Z0-9?]+)?\}")]
        private static partial Regex MyRegex();
    }
}
