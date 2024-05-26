using AnjUx.ORM.Classes;
using AnjUx.ORM;
using AnjUx.Shared.Attributes;
using AnjUx.Shared.Extensions;
using AnjUx.Shared.Models;
using System.Reflection;
using AnjUx.Services;
using System.Text;

namespace AnjUx.Server.Services
{
    public class GenericSearchService(DBFactory? factory = null, string? nomeUsuario = null) : BaseDBService<BaseModel>(factory, nomeUsuario)
    {
        public async Task<List<T>> GenericSearch<T>(string? filter) where T : BaseModel
        {
            List<string> propriedadesFiltrar = [];

            foreach (PropertyInfo prop in typeof(T).GetProperties())
                if (prop.GetCustomAttribute<SearchDisplayAttribute>() != null)
                    propriedadesFiltrar.Add(prop.Name);

            QueryModel<T> query = new("T");

            if (propriedadesFiltrar.IsNullOrEmpty())
                throw new Exception($"Nenhuma propriedade para filtrar no tipo \"{typeof(T).Name}\"!");


            if (!filter.IsNullOrWhiteSpace())
            {
                StringBuilder filtro = new();
                filtro.AppendLine("AND (");
                foreach (string prop in propriedadesFiltrar)
                {
                    string endstring = propriedadesFiltrar.IndexOf(prop) == propriedadesFiltrar.Count - 1 ? ")" : "OR ";
                    filtro.AppendLine($"T.{prop} LIKE '%{filter}%' {endstring}");
                }

                query.Filtros.Add(new Filtro(filtro.ToString()));
            }

            query.Top = 100;

            QueryExecutor executor = new(this);
            return await executor.Listar(query);
        }
    }
}
