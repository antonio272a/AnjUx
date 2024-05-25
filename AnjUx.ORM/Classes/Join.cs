using AnjUx.Shared.Extensions;
using AnjUx.ORM.Attributes;
using AnjUx.ORM.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using AnjUx.Shared.Interfaces;

namespace AnjUx.ORM.Classes
{
    public enum JoinTipo: int
    {
        Inner = 0,
        Left = 1,
        Right = 2
    }

    public enum RelacaoTipo: int
    {
        Pai = 0,
        Filho = 1
    }

    public class Join
    {
        public JoinTipo? Tipo { get; set; }
        public RelacaoTipo? Relacao { get; set; }
        public string? Alias { get; set; }
        public string? Campo { get; set; }
        public string? Propriedade { get; set; }
        private string? Tabela { get; set; }
        public List<Join> Joins { get; set; } = new();
        public Type Classe { get; set; }
        private Join? Pai { get; set; }
        public List<Filtro> Filtros { get; set; } = new();

        #region Construtores

        public Join(Type classe)
        {
            Classe = classe;
        }
        public Join(Type classe, JoinTipo tipo)
        {
            Classe = classe;
            Tipo = tipo;
        }
        public Join(Type classe, RelacaoTipo relacao)
        {
            Classe = classe;
            Relacao = relacao;
        }

        public Join(Type classe, string campo)
        {
            Classe = classe;
            Campo = campo;
        }

        public Join(Type classe, JoinTipo tipo, string campo)
        {
            Classe = classe;
            Tipo = tipo;
            Campo = campo;
        }

        public Join(Type classe, RelacaoTipo relacao, JoinTipo tipo)
        {
            Classe = classe;
            Relacao = relacao;
            Tipo = tipo;
        }

        public Join (Type classe, RelacaoTipo relacao, string? alias = null, JoinTipo? tipo = null, string? campo = null, string? propriedade = null) 
        {
            Relacao = relacao;
            Classe = classe;
            Tipo = tipo;
            Alias = alias;
            Campo = campo;
            Propriedade = propriedade;
        }

        public Join (Type classe, RelacaoTipo? relacao = null, string? alias = null)
        {
            Classe = classe;
            Relacao = relacao;
            Alias = alias;
        }

        #endregion

        #region Métodos de preenchimento

        /// <summary>
        /// Tenta preencher todos os campos do objeto.
        /// Caso algum campo já esteja preenchido, não sobreescreve
        /// </summary>
        public void PreencherPropriedadesVazias(Type? tipoAlvo = null)
        {
            if(tipoAlvo != null)
                EncontrarRelacaoTipo(tipoAlvo);

            EncontrarTabela();
            EncontrarAlias();
            EncontrarJoinTipo();
            EncontrarCampo();
        }

        /// <summary>
        /// Monta a tabela com o Schema para o join
        /// </summary>
        /// <exception cref="CredilojaException"></exception>
        private void EncontrarTabela()
        {
            var atributo = (DBTableAttribute?)Classe.GetCustomAttribute(typeof(DBTableAttribute));

            if (atributo == null) throw new Exception("Indique uma tabela válida");

            string schemaString = atributo.Schema.IsNullOrWhiteSpace() ? "" : $"{atributo.Schema}.";
            Tabela = $"{schemaString}{atributo.Table}";
        }

        /// <summary>
        /// Tenta encontrar o campo que é necessário para o join
        /// </summary>
        private void EncontrarCampo()
        {
            if (Campo == null && Relacao != null)
            {
                switch (Relacao)
                {
                    case RelacaoTipo.Pai:
                        Campo = Classe.Name;
                        break;
                    case RelacaoTipo.Filho:
                        // Deixamos nulo pois é decidido na hora de criar os joins
                        Campo = null;
                        break;
                }
            }
        }

        /// <summary>
        /// Monta o Alias baseado no nome da classe.
        /// Esse alias ainda pode ser alterado na hora de montagem da query para evitar duplicidade
        /// </summary>
        private void EncontrarAlias()
        {
            if (Alias == null) Alias = Classe.Name.GetUpperCaseChars();
        }
        
        /// <summary>
        /// Coloca o tipo de Join baseado na relacao
        /// </summary>
        private void EncontrarJoinTipo()
        {   
            if (Tipo == null && Relacao != null)
            {

                switch (Relacao)
                {
                    case RelacaoTipo.Pai:
                        // Caso possua pai e o pai seja Left, continua como left
                        if (Pai != null && Pai!.Tipo == JoinTipo.Left) Tipo = JoinTipo.Left; 
                        else Tipo = JoinTipo.Inner;
                        break;
                    case RelacaoTipo.Filho:
                        Tipo = JoinTipo.Left;
                        break;
                }
            }
        }

        private void EncontrarRelacaoTipo(Type classeAlvo)
        {
            if(Relacao == null)
            {
                List<PropertyInfo> props = Classe.GetProperties().Where(prop => prop.GetCustomAttribute(typeof(NotMappedAttribute)) == null).ToList();

                // Caso o nosso join tenha alguma propriedade que tenha o mesmo tipo da classe alvo, significa que esse join é um child
                if (props.Any(prop => prop.Name == classeAlvo.Name)) 
                    Relacao = RelacaoTipo.Filho;
                else 
                    Relacao = RelacaoTipo.Pai;
            }
        }

        public Join AddJoins(params Join[] joins)
        {
            foreach (Join join in joins)
            {
                Joins.Add(join);
            }

            return this;
        }

        public void AddFiltros(params Filtro[] filtros)
        {
            Filtros.AddRange(filtros);
        }

        #endregion

        #region Métodos de Verificação

        /// <summary>
        /// Verifica se o Alias já está sendo utilizado para montar a query e altera o mesmo até conseguir um alias livre
        /// Caso tenha joins internos faz o processo recursivamente
        /// </summary>
        /// <param name="aliasUtilizados"></param>
        public void VerificarAlias(List<string> aliasUtilizados)
        {
            EncontrarAlias();

            int counter = 1;
            string originalAlias = Alias!;

            while (aliasUtilizados.Contains(Alias!))
            {
                Alias = originalAlias + counter;
                counter++;
            }

            aliasUtilizados.Add(Alias!);

            // Verifica todos os joins internos
            foreach (Join join in Joins)
            {
                join.VerificarAlias(aliasUtilizados);
            }
        }

        #endregion

        #region Metodos ToString

        public string JoinsParaString(string aliasAlvo, Type tipoAlvo, Join? pai = null)
        {
            Pai = pai;

            EncontrarRelacaoTipo(tipoAlvo);

            PreencherPropriedadesVazias();

            // Caso o campo não tenha sido informado e seja um filho, pegamos o campo pelo tipo do alvo
            if(Campo == null && Relacao == RelacaoTipo.Filho && tipoAlvo != null)
                Campo = tipoAlvo!.Name;

            var sb = new StringBuilder();

            sb.Append('\t');

            switch(Tipo)
            {
                case JoinTipo.Inner:
                    sb.Append("INNER JOIN");
                    break;

                case JoinTipo.Left:
                    sb.Append("LEFT JOIN");
                    break;

                case JoinTipo.Right:
                    sb.Append("RIGHT JOIN");
                    break;
            }
            sb.Append(' ');
            sb.Append($"{Tabela} {Alias} ON");
            sb.Append(' ');

            switch(Relacao)
            {
                case RelacaoTipo.Pai:
                    sb.AppendLine($"{Alias}.ID = {aliasAlvo}.{Campo}");
                    break;
                    
                case RelacaoTipo.Filho:
                    sb.AppendLine($"{aliasAlvo}.ID = {Alias}.{Campo}");
                    break;
            }

            if (!Filtros.IsNullOrEmpty())
            {
                foreach(Filtro filtro in Filtros)
                {
                    sb.Append(filtro.ToString());
                }
            }

            if(!Joins.IsNullOrEmpty())
            {
                foreach(Join join in Joins!)
                {
                    sb.Append(join.JoinsParaString(Alias!, Classe, this));
                }

            }

            return sb.ToString();
        }

        public string CamposParaString<T>(QueryModel<T> model) where T : IDbModel
        {
            StringBuilder sb = new();

            string camposPrincipais = QueryBuilder.CamposParaString(model, Classe, Alias!);
            sb.Append(camposPrincipais);

            foreach(Join join in Joins)
            {
                sb.AppendLine(", ");
                sb.Append(join.CamposParaString(model));
            }

            return sb.ToString();
        }

        #endregion

    }
}
