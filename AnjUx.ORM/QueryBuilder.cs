using BasicSQLFormatter;
using AnjUx.Shared.Extensions;
using Force.DeepCloner;
using Newtonsoft.Json;
using AnjUx.ORM.Classes;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using AnjUx.Shared.Interfaces;
using AnjUx.Shared.Attributes;


namespace AnjUx.ORM
{
    internal enum OperacaoSQLTipo : int
    {
        Insert = 1,
        Update = 2,
    }

    public class QueryBuilder
    {
        public string? UltimoSQL { get; private set; }

        public bool SQLAtualizar<T>(T objetoAtualizado, T objetoAntigo, string tabela, string nomeUsuario)
            where T : IBaseModel
        {
            var atualizacoes = new List<string>();
            var dicionarioAtualizado = DicionarioDeObjeto(objetoAtualizado);
            var dicionarioAntigo = DicionarioDeObjeto(objetoAntigo);

            // Vamos verificar quais campos sofreram alteração
            foreach (var entrada in dicionarioAtualizado)
            {
                var valorAtualizado = entrada.Value;
                var valorAntigo = dicionarioAntigo[entrada.Key];

                if (valorAtualizado != valorAntigo)
                    atualizacoes.Add(entrada.Key);
            }

            // Se não houve atualização alguma, então retornamos true e a propriedade "UltimoSQL" com valor "null"
            if (atualizacoes.Count <= 0)
                return false;

            atualizacoes.Add("Updated");
            atualizacoes.Add("UpdateUser");

            // Adicionamos novos valores para os campos de Update
            dicionarioAtualizado.Add("Updated", "CURRENT_TIMESTAMP");
            dicionarioAtualizado.Add("UpdateUser", $"\'{nomeUsuario}\'");

            var sql = new StringBuilder();
            sql.AppendLine($"UPDATE");
            sql.AppendLine($"\t{tabela}");
            sql.AppendLine($"SET");

            var valores = new StringBuilder();

            foreach (var atualizacao in atualizacoes)
            {
                if (valores.Length > 0)
                    valores.AppendLine(", ");

                var campo = atualizacao;
                var valor = dicionarioAtualizado[campo];

                valores.Append($"\t{campo} = {valor}");
            }

            sql.AppendLine(valores.ToString());

            sql.AppendLine("WHERE");
            sql.AppendLine($"\tID = {objetoAtualizado.ID}");

            UltimoSQL = sql.ToString();

            return true;
        }

        public bool SQLInserir<T>(T objeto, string tabela, string nomeUsuario)
            where T : IDbModel
        {
            var naoNulos = new List<string>();
            var dicionario = DicionarioDeObjeto(objeto);

            dicionario.Add("Inserted", "CURRENT_TIMESTAMP");
            dicionario.Add("InsertUser", $"\'{nomeUsuario}\'");

            // Adicionamos os campos de Update apenas se T herdar de BaseModel
            if (typeof(T).GetInterfaces().Any(i => i == typeof(IBaseModel)))
            {
                dicionario.Add("Updated", "CURRENT_TIMESTAMP");
                dicionario.Add("UpdateUser", $"\'{nomeUsuario}\'");

                if (dicionario.Count == 4)
                    return false;
            }
            else
            {
                if (dicionario.Count == 2)
                    return false;
            }

            // Selecionamos os campos das entradas que não possuem um valor "null"
            foreach (var entrada in dicionario)
            {
                if (entrada.Value == "null")
                    continue;

                naoNulos.Add(entrada.Key);
            }

            if (naoNulos.IsNullOrEmpty())
                return false;

            // Vamos montar o SQL
            var sb = new StringBuilder();
            int tamanho;

            // 1. Cabeçalho do Insert
            sb.AppendLine($"INSERT INTO {tabela}");

            // 2. Campos que terão valores inseridos
            var campos = new StringBuilder();
            campos.AppendLine("(");
            tamanho = campos.Length;

            foreach (var naoNulo in naoNulos)
            {
                if (campos.Length > tamanho)
                    campos.AppendLine(", ");

                campos.Append($"\t{naoNulo}");
            }

            campos.AppendLine($"\n)");

            sb.Append(campos.ToString());

            // 3. Valores dos campos (sem nenhum "null")
            sb.AppendLine("VALUES");

            var valores = new StringBuilder();
            valores.AppendLine("(");
            tamanho = valores.Length;

            foreach (var entrada in dicionario)
            {
                if (!naoNulos.Contains(entrada.Key))
                    continue;

                if (valores.Length > tamanho)
                    valores.AppendLine(", ");

                valores.Append($"\t{entrada.Value}");
            }

            valores.AppendLine($"\n)");

            sb.Append(valores.ToString());

            UltimoSQL = sb.ToString();

            return true;
        }

        private static Dictionary<string, string?> DicionarioDeObjeto<T>(T objeto)
        {
            var dicionario = new Dictionary<string, string?>();

            var propriedades = objeto!.GetType().GetProperties();

            if (propriedades == null)
                return dicionario;

            foreach (var propriedade in propriedades)
            {
                // Se a Propriedade possuir o atributo NotMapped, a pulamos
                if (propriedade.GetCustomAttributes(false).All(a => a.GetType() != typeof(DBFieldAttribute)))
                    continue;

                var propriedadeNome = propriedade.Name;
                
                // Se for uma das propriedades de BaseModel, também pulamos
                if (propriedadeNome.In("ID", "Inserted", "InsertUser", "Updated", "UpdateUser"))
                    continue;

                // Convertemos o valor da propriedade para uma string pronta para ser usada no SQL
                var valorString = ObjetoPropriedadeValorParaString(objeto, propriedadeNome);

                // Adicionamos o nome do campo e o valor
                // da propriedade como string no dicionário
                dicionario.Add(propriedadeNome, $"{valorString}");
            }

            return dicionario;
        }
    
        public bool SQLSelect<T>(QueryModel<T> model) where T: IDbModel
        {
            StringBuilder sb = new();

            var paginacao = model.ResultadosPorPagina.GetValueOrDefault() > 0 && model.Pagina.GetValueOrDefault() > 0;

            sb.AppendLine($"SELECT");

            List<PropertyInfo> props = typeof(T).GetProperties().Where(prop => prop.GetCustomAttribute(typeof(DBFieldAttribute)) != null).ToList();

            if (!model.UtilizarSerializaveis)
                props = props.Where(prop => prop.GetCustomAttribute(typeof(SerializableAttribute)) == null).ToList();

            if (props.IsNullOrEmpty()) 
                return false;

            VerificarAlias(model);

            string camposPrincipais = CamposParaString(model, typeof(T), model.Alias);

            sb.Append(camposPrincipais);

            foreach (Join join in model.Joins)
            {
                sb.AppendLine(", ");
                sb.Append(join.CamposParaString(model));
            }
            sb.AppendLine();

            MontarJoins(ref sb, model);
            
            MontarFiltros(ref sb, model);

            // Paginação
            if (paginacao)
            {
                var resultados = model.ResultadosPorPagina;
                var ignorar = resultados * (model.Pagina - 1);

                SubQuery(null, ignorar, resultados);
            }
            else if (model.Top.GetValueOrDefault() > 0)
            {
                SubQuery(model.Top, null, null);
            }

            MontarOrderBy(ref sb, model);

            UltimoSQL = new SQLFormatter(sb.ToString().ToOneLine()).Format();

            return true;

            void SubQuery(int? top, int? ignorar, int? resultados)
            {
                var subModel = DeepClonerExtensions.DeepClone(model);
                subModel.Alias = $"{model.Alias}";
                var sb1 = new StringBuilder();

                var distinct = subModel.PaginacaoOrdenacao == null;
                var paginacaoExterna = !distinct;

                sb1.Append("SELECT");

                if (distinct)
                    sb1.Append(" DISTINCT");

                if (top.GetValueOrDefault() > 0)
                    sb1.AppendLine($" TOP({top})");
                else
                    sb1.AppendLine();

                var subModelID = $"{subModel.Alias}.ID";

                sb1.AppendLine($"\t\t{subModelID}");

                MontarJoins(ref sb1, subModel);
                MontarFiltros(ref sb1, subModel);

                var orderBy = MontarOrderBy(model) ?? $"\t{subModel.Alias}.ID";

                bool paginado = ignorar != null && resultados != null;

                if (paginado)
                {
                    if (paginacaoExterna)
                    {
                        string? agrupador = string.Empty;
                        
                        if (!model.GroupsBy.IsNullOrEmpty())
                        {
                            foreach (var groupBy in model.GroupsBy)
                            {
                                if (agrupador != string.Empty)
                                    agrupador += ", ";

                                agrupador += groupBy;
                            }
                        }
                        else
                        {
                            var agrupadorAlias = subModel.PaginacaoOrdenacao!.Join?.Alias ?? subModel.PaginacaoOrdenacao?.CampoCompleto;
                            var agrupadorCampo = subModel.PaginacaoOrdenacao!.Campo;

                            agrupador = agrupadorAlias.IsNullOrWhiteSpace() || agrupadorCampo.IsNullOrWhiteSpace()
                                ? subModel.PaginacaoOrdenacao!.CampoCompleto
                                : $"{agrupadorAlias}.{agrupadorCampo}";
                        }
                        

                        sb1.AppendLine("GROUP BY");
                        sb1.AppendLine($"\t{subModelID}, {agrupador}");

                        orderBy = $"\t{subModel.PaginacaoOrdenacao}";
                    }

                    sb1.AppendLine("ORDER BY");
                    sb1.AppendLine(orderBy);
                    sb1.AppendLine("OFFSET");
                    sb1.AppendLine($"\t{ignorar} ROWS");
                    sb1.AppendLine("FETCH NEXT");
                    sb1.AppendLine($"\t{resultados} ROWS ONLY");
                }

                var subQuery = sb1.ToString();

                sb.AppendLine($"\tAND {model.Alias}.ID IN");
                sb.AppendLine($"\t(");
                sb.AppendLine($"\t\t{subQuery}");
                if(!paginado) MontarOrderBy(ref sb, model);
                sb.AppendLine($"\t)");

                if(model.OrdersBy.IsNullOrEmpty())
                {
                    sb.AppendLine("ORDER BY");
                    sb.AppendLine($"{orderBy}");
                }
            }
        }

        public bool SQLCount<T>(QueryModel<T> model) where T : IDbModel
        {
            StringBuilder sb = new();

            sb.AppendLine("SELECT");

            VerificarAlias(model);

            sb.AppendLine($"COUNT(DISTINCT {model.Alias}.ID)");

            MontarJoins(ref sb, model);

            MontarFiltros(ref sb, model);

            UltimoSQL = sb.ToString();

            return true;
        }

        private static string? MontarOrderBy<T>(QueryModel<T> model) where T : IDbModel
        {
            if (model.OrdersBy.IsNullOrEmpty()) return null;

            StringBuilder sb = new();
            MontarOrderBy(ref sb, model);

            return sb.ToString();
        }

        private static void MontarOrderBy<T>(ref StringBuilder sb, QueryModel<T> model) where T : IDbModel
        {
            if (!model.OrdersBy.IsNullOrEmpty())
            {
                sb.AppendLine("ORDER BY");

                var order = string.Empty;

                foreach (var orderBy in model.OrdersBy)
                {
                    if (!order.IsNullOrWhiteSpace())
                        order += ",\n";

                    order += orderBy.ToString();
                }

                sb.AppendLine(order);
            }
        }

        private static void MontarJoins<T>(ref StringBuilder sb, QueryModel<T> model) where T: IDbModel
        {
            sb.AppendLine("FROM");

            sb.AppendLine($"\t{model.Tabela} {model.Alias}");

            foreach (Join join in model.Joins)
            {
                var joinInterno = join.JoinsParaString(model.Alias, typeof(T));
                sb.Append(joinInterno);
            }
        }

        private static void MontarFiltros<T>(ref StringBuilder sb, QueryModel<T> model) where T: IDbModel
        {
            sb.AppendLine("WHERE 1 = 1");

            foreach (Filtro filtro in model.Filtros)
            {
                sb.AppendLine(filtro.ToString());
            }
        }

        private static void VerificarAlias<T>(QueryModel<T> model) where T: IDbModel
        {
            List<string> aliasUtilizados = new() { model.Alias };

            foreach(Join join in model.Joins)
            {
                join.VerificarAlias(aliasUtilizados);
            }
        }

        internal static string CamposParaString<T>(QueryModel<T> model, Type classe, string alias) where T: IDbModel
        {
            List<PropertyInfo> props = classe.GetProperties().Where(prop => prop.GetCustomAttribute(typeof(DBFieldAttribute)) != null).ToList();

            if (!model.UtilizarSerializaveis)
                props = props.Where(prop => prop.GetCustomAttribute(typeof(SerializableAttribute)) == null).ToList();

            // Está exception nunca deveria acontecer
            if (props.IsNullOrEmpty()) throw new Exception("Classe informada para construção de busca não possui campos mapeados");

            StringBuilder sb = new();

            foreach (PropertyInfo prop in props)
            {
                string fieldName = $"{alias}{prop.Name}";
                
                if (sb.Length > 0) sb.AppendLine(", ");
                sb.Append($"\t{alias}.{prop.Name}");

                
                int IndexAtual = model.ReferenciaCampos.Count;
                model.ReferenciaCampos.Add(fieldName, IndexAtual);
            }

            foreach (CustomField customField in model.CustomFields.Where(cf => cf.AlvoAlias == alias))
            {
                if (sb.Length > 0)
                    sb.AppendLine(", ");

                sb.Append($"{customField.CondicaoPreenchimento} AS {customField.CampoAlias}");

                int indexAtual = model.ReferenciaCampos.Count;
                model.ReferenciaCampos.Add(customField.CampoAlias, indexAtual);
            }

            return sb.ToString();
        }

        public static string ObjetoPropriedadeValorParaString(object objeto, string propriedadeNome)
        {
            // Verificamos se o objeto informado possui uma propriedade com o nome informado
            var tipo2 = objeto.GetType();
            
            var propriedade = tipo2.GetProperty(propriedadeNome);

            if (propriedade == null)
                throw new Exception($"Propriedade inexistente: \"{propriedadeNome}\".");

            // Capturamos o valor da propriedade do objeto
            var valor = propriedade!.GetValue(objeto);

            if (valor == null)
                return "NULL";

            // Capturamos os atributos da propriedade
            var atributos = propriedade.GetCustomAttributes(false);

            // É uma propriedade com o atributo "Serializavel"?
            if (atributos.Any(a => a.GetType() == typeof(SerializableAttribute)))
            {
                var json = JsonConvert.SerializeObject(valor);
                json = json.Replace("'", "''");
                return $"\'{json}\'";
            }

            // TODO: (Jeferson) Implementar suporte a colunas to tipo Binario do SQLServer
            //if (binario)
            //{
            //    var json = JsonConvert.SerializeObject(valor);
            //    var bin = Encoding.UTF8.GetBytes(json);
            //    dicionario.Add(campo, $"CONVERT(varbinary(MAX), \'{bin}\'");
            //    continue;
            //}

            // Capturamos o tipo não nulo da propriedade
            Type tipo = propriedade.NonNullableType();

            // É de um tipo que possui ID?
            if (tipo.IsSubclassOf(typeof(IDbModel)))
            {
                var id = valor!.GetType().GetProperty("ID")?.GetValue(valor, null) as long?;
                var idString = id != null ? id.ToString() : "null";
                return $"{idString}";
            }

            return ValorPrimitivoParaString(valor!);
        }

        public static string ValorPrimitivoParaString(object valor)
        {
            Type tipo = valor.GetType();

            // É um enum?
            if (tipo.IsEnum)
            {
                var subtipo = Enum.GetUnderlyingType(tipo);
                var enumerador = Convert.ChangeType(valor, subtipo);
                
                return $"{enumerador}";
            }

            if(tipo.IsListOfEnum())
            {
                // Get the specific Enum type of the list
                Type enumType = tipo.GetGenericArguments()[0];

                // Get the extension method info
                var method = typeof(IEnumerableExtensions).GetMethod(nameof(IEnumerableExtensions.ToEnumCommaString));

                // Create a closed constructed method from the generic method definition
                var genericMethod = method!.MakeGenericMethod(enumType);

                // Invoke the method
                string result = (string)genericMethod.Invoke(null, new object[] { valor, false })!;

                return result;
            }

            // Atenção para não colocar esse método antes da conferência de Lista de Enum, pois o método abaixo também capturaria uma lista de enum
            if (tipo.IsArray || tipo.IsList())
            {
                var array = "";

                // Se é um List<>, convertemos para um array
                if (tipo.IsList())
                {
                    MethodInfo toArrayMethod = typeof(Enumerable).GetMethod("ToArray")!.MakeGenericMethod(tipo.GetGenericArguments()[0]);
                    valor = toArrayMethod.Invoke(null, new object[] { valor })!;
                }

                foreach (var elemento in (Array)valor)
                {
                    if (elemento == null)
                        continue;

                    var elementoString = ValorPrimitivoParaString(elemento);

                    if (elementoString.IsNullOrWhiteSpace())
                        continue;

                    if (!array.IsNullOrWhiteSpace())
                        array += ", ";

                    array += elementoString;
                }

                return $"{array}";
            }

            // É um tipo "básico" do C#?
            string tipoNome = tipo.Name;

            switch (tipoNome)
            {
                case "Char":
                case "String":
                    var cadeia = ((string)valor!).Replace("'", "''");
                    return $"\'{cadeia}\'";

                case "Decimal":
                case "Double":
                    var flutuante = valor!.ToString()!.Replace(',', '.');
                    return $"{flutuante}";

                case "DateTime":
                    var data = ParaStringDeSQL((DateTime)valor);
                    return $"{data}";

                case "Boolean":
                    var booleano = Convert.ToByte(valor).ToString();
                    return $"{booleano}";

                default:
                    return $"{valor}";
            }
        }

        private static string ParaStringDeSQL(DateTime data)
        {
            return $"CONVERT(DATETIME, '{data:yyyy-MM-dd HH:mm:ss.fff}', 21)";
        }
    }
}