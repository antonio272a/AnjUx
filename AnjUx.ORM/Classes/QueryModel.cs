using AnjUx.Shared.Extensions;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using AnjUx.Shared.Interfaces;
using AnjUx.Shared.Attributes;

namespace AnjUx.ORM.Classes
{
    public class QueryModel
    {
        public string Alias { get; set; }

        public QueryModel(Type model, string? alias = null)
        {
            Alias = alias.IsNullOrWhiteSpace()
                ? model.Name.GetUpperCaseChars()
                : alias!;
        }
    }

    public class QueryModel<T> : QueryModel
        where T: IDbModel
    {
        public List<Join> Joins { get; set; } = new List<Join>();
        public List<Filtro> Filtros { get; set; } = new List<Filtro>();
        public List<OrderBy> OrdersBy { get; set; } = new List<OrderBy>();
        public List<CustomField> CustomFields { get; set; } = new();
        public List<string> GroupsBy { get; set; } = new();
        public string Tabela { get; set; }
        public int? Top { get; set; }
        public int? ResultadosPorPagina { get; set; }
        public int? Pagina { get; set; }

        /// <summary>
        /// Utilizado para guardar como chave o campo e como valor o índice que ele terá no DataReader
        /// </summary>
        public Dictionary<string, int> ReferenciaCampos { get; set; } = new();

        public bool UtilizarSerializaveis { get; set; } = true;

        /// <summary>
        /// Ignorado se não forem informados ResultadosPorPagina e Pagina.
        /// </summary>
        /// <remarks>
        /// <b>ATENÇÃO</b>: Inicializar com construtor que recebe Join e Campo.
        /// </remarks>
        public OrderBy? PaginacaoOrdenacao { get; set; }

        public QueryModel(string? alias = null) : base(typeof(T), alias)
        {
            var atributo = typeof(T).GetCustomAttribute<DBTableAttribute>();
            if (atributo == null) 
                throw new Exception("Indique uma tabela válida");            

            string schemaString = atributo.Schema.IsNullOrWhiteSpace() ? "" : $"{atributo.Schema}.";
            Tabela = $"{schemaString}{atributo.Table}";
        }

        public void AddJoins(params Join[] joins)
        {
            foreach(Join join in joins)
            {
                Joins.Add(join);
            }
        }

        public void AddCustomField(Expression<Func<T, object?>> propriedade, string condicaoPreenchimento)
        {
            string campoNome = GetCampoNome(propriedade);

            CustomFields.Add(new(Alias, campoNome, condicaoPreenchimento));
        }

        public void AddCustomField<X>(Expression<Func<X, object?>> propriedade, string condicaoPreenchimento, string alias)
            where X : IDbModel
        {
            string campoNome = GetCampoNome(propriedade);

            CustomFields.Add(new(alias, campoNome, condicaoPreenchimento));
        }

        private string GetCampoNome<X>(Expression<Func<X, object?>> propriedade)
            where X : IDbModel
        {
            PropertyInfo? propriedadeInfo = null;

            Expression body = propriedade.Body;
            if (body is MemberExpression member)
                propriedadeInfo = (PropertyInfo)member.Member;
            else if (body is UnaryExpression unary && unary.Operand is MemberExpression nestedMember)
                propriedadeInfo = (PropertyInfo)nestedMember.Member;

            if (propriedadeInfo == null)
                throw new Exception();

            var atributoNaoMapeado = propriedadeInfo.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(NotMappedAttribute));
            if (atributoNaoMapeado == null)
                throw new Exception($"A Propriedade informada como CustomField deve possuir o atributo \"{nameof(NotMappedAttribute)}\".");

            if (propriedadeInfo.CustomAttributes.Any(a => a.AttributeType == typeof(DBChildrenAttribute)))
                throw new Exception($"A Propriedade informada com CustomField não deve possuir o atributo \"{nameof(DBChildrenAttribute)}\"");

            return propriedadeInfo.Name;
        }
    }
}
