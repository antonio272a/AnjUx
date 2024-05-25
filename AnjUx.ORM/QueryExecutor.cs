using Dapper;
using AnjUx.Shared.Extensions;
using AnjUx.Shared.Tools;
using Newtonsoft.Json;
using AnjUx.ORM.Classes;
using AnjUx.ORM.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AnjUx.Shared.Interfaces;
using AnjUx.Shared.Attributes;

namespace AnjUx.ORM
{
    public class QueryExecutor : IDBFactoryConsumer
    {
        public DBFactory DBFactory { get; set; }
        public string? NomeUsuario { get; }
        public string? UltimoSQL { get; set; }
        private bool UtilizarSerializaveis { get; set; } = true;
        private readonly Dictionary<string, List<PropertyInfo>> PropriedadesPaisCache = [];
        private readonly Dictionary<string, List<PropertyInfo>> PropriedadesFilhoCache = [];
        private Dictionary<string, List<CustomField>> CustomFieldsCache = [];
        private Dictionary<string, int>? ReferenciaCampos { get; set; }

        public QueryExecutor(IDBFactoryConsumer consumidor)
        {
            NomeUsuario = consumidor.NomeUsuario;
            
            if (consumidor.DBFactory == null)
                throw new ArgumentException("DBFactory can't be null");

            DBFactory = consumidor.DBFactory;

        }

        public async Task<bool> AbrirComJoins<T>(T objeto, QueryModel<T> model) 
            where T : IDbModel
        {
            // Adiciona o Filtro de ID do alias principal
            model.Filtros.Add(new Filtro(FiltroTipo.And, OperadorTipo.Igual, $"{model.Alias}.ID", objeto.ID!));

            List<T> lista = await Listar(model);
            T? objetoNovo = lista.FirstOrDefault();
            
            if (!objetoNovo.IsPersisted()) return false;

            // Aqui só vamos manter do objeto antigo as props virtuais, ou seja, as que não são mapeadas e também não são children.
            List<PropertyInfo> virtuals = objeto.Properties(typeof(DBFieldAttribute), true).Where(prop => prop.GetCustomAttribute(typeof(DBChildrenAttribute)) == null).ToList();

            List<PropertyInfo> props = objeto.Properties();

            // Passamos todas as props do objeto novo para o que foi passado para o Open
            foreach(PropertyInfo prop in props)
            {
                // Se é uma virtual, não é sobreescrita, já que seria sobreescrita por null
                if (virtuals.Contains(prop)) continue;

                prop.SetValue(objeto, prop.GetValue(objetoNovo));
            }

            return true;
        }

        public async Task<int> Count<T>(QueryModel<T> model) where T: IDbModel
        {
            DBFactory.NovaTransacao(out bool minhaTransacao);

            try
            {
                QueryBuilder queryBuilder = new();

                if (!queryBuilder.SQLCount(model)) 
                    throw new Exception("Houve um erro ao montar o SQL");

                UltimoSQL = queryBuilder.UltimoSQL;
                ReferenciaCampos = model.ReferenciaCampos;

                int count = int.Parse((await DBFactory.Connection!.ExecuteScalarAsync(UltimoSQL!, transaction: DBFactory.Transaction!))!.ToString()!);

                DBFactory.CommitTransacao(minhaTransacao);

                return count;
            }
            catch (Exception)
            {
                DBFactory.RollbackTransacao(minhaTransacao);

                throw;
            }
            finally
            {
                DBFactory.FecharConexao(minhaTransacao);
            }
        }

        public async Task<List<T>> Listar<T>(QueryModel<T> model)
            where T: IDbModel
        {
            DBFactory.NovaTransacao(out bool minhaTransacao);

            try
            {
                QueryBuilder queryBuilder = new();

                if (!queryBuilder.SQLSelect(model)) 
                    throw new Exception("Houve um erro ao montar o SQL");

                if (!model.CustomFields.IsNullOrEmpty())
                    CustomFieldsCache = model.CustomFields.ToGroupedDictionary(c => c.AlvoAlias);

                UltimoSQL = queryBuilder.UltimoSQL;
                UtilizarSerializaveis = model.UtilizarSerializaveis;
                ReferenciaCampos = model.ReferenciaCampos;

                List<T> response = [];
                Dictionary<long, T> objetosSalvos = [];

                IDataReader dr = await DBFactory.Connection!.ExecuteReaderAsync(UltimoSQL!, transaction: DBFactory.Transaction, commandTimeout: 120);

                // Monta os objetos de acordo com o Model
                while (dr.Read())
                {
                    var objeto = Activator.CreateInstance<T>();

                    PreencherObjeto(objeto, dr, model.Alias, model.Joins);

                    var objetoSalvo = objetosSalvos.ValueIfKeyExists(objeto.ID!.Value);

                    // Caso não tenha passado por esse objeto, salva, caso contrário, mescla os childrens
                    if (!objetoSalvo.IsPersisted())
                    {
                        response.Add(objeto);
                        objetosSalvos.Add(objeto.ID!.Value, objeto);
                    }
                    else
                        MesclarFilhos(objetoSalvo!, objeto);
                }

                // Fechamos o Data Reader para evitar travar a conexão
                dr.Dispose();

                DBFactory.CommitTransacao(minhaTransacao);

                return response;
            }
            catch (Exception ex)
            {
                DBFactory.RollbackTransacao(minhaTransacao);

                throw ex.ReThrow();
            }
            finally
            {
                DBFactory.FecharConexao(minhaTransacao);
            }            
        }

        private void MesclarFilhos(object objetoSalvo, object objetoNovo)
        {
            Type tipoObjetoSalvo = objetoSalvo.GetType();
            Type tipoNovoObjeto = objetoNovo.GetType();

            if (tipoObjetoSalvo != tipoNovoObjeto)
            {
                // Esse erro nunca deverá explodir, mas o seguro morreu de velho...
                throw new ArgumentException("Objetos precisam ser do mesmo tipo.");
            }

            List<PropertyInfo> propsPais = PegarPropriedadesPais(tipoObjetoSalvo);
            List<PropertyInfo> propsFilhos = PegarPropriedadesFilhos(tipoObjetoSalvo);

            IDbModelComparator comparer = new();

            foreach (PropertyInfo prop in propsPais)
            {
                // Caso não seja um DBModel, não precisamos mesclar
                if (!prop.PropertyType.IsSubclassOf(typeof(IDbModel))) continue;

                var subObjetoSalvo = prop.GetValue(objetoSalvo)!;
                var subObbjetoNovo = prop.GetValue(objetoNovo)!;

                // Caso esteja nulo, não precisamos mesclar;
                if (subObjetoSalvo == null || subObbjetoNovo == null) continue;

                MesclarFilhos(subObjetoSalvo, subObbjetoNovo);
            }

            foreach (PropertyInfo prop in propsFilhos)
            {
                Type listGenericType = prop.PropertyType.GetGenericArguments()[0];

                // Aqui utilizamos o helper para poder trabalhar com a tipagem genérica.
                MethodInfo helperMethod = typeof(QueryExecutor).GetMethod("MesclarFilhosHelper", BindingFlags.NonPublic | BindingFlags.Instance)!;
                MethodInfo genericHelperMethod = helperMethod.MakeGenericMethod(listGenericType);
                var resultSet = genericHelperMethod.Invoke(this, [prop.GetValue(objetoSalvo), prop.GetValue(objetoNovo), comparer]);

                prop.SetValue(objetoSalvo, resultSet);
            }
        }

        private List<T> MesclarFilhosHelper<T>(IEnumerable<T> filhosSalvos, IEnumerable<T> novosFilhos, IEqualityComparer<T> comparer) where T : IDbModel
        {
            List<T> resultSet = [];

            if (filhosSalvos != null && filhosSalvos.Any())
            {
                HashSet<T> filhosMesclados = new (comparer);

                // Adiciona todos os já salvos no HashSet
                foreach (var filhoSalvo in filhosSalvos)
                {
                    filhosMesclados.Add(filhoSalvo);
                }

                // Caso tenha novos childrens, adiciona eles no HashSet
                if (novosFilhos != null)
                {
                    foreach (var filho in novosFilhos)
                    {
                        filhosMesclados.Add(filho);
                    }

                    // Mescla recursivamente os childrens
                    foreach (var filhoSalvo in filhosSalvos)
                    {
                        // Se existe um novo filho que tenha o mesmo ID de do filho salvo, precisamos mesclar
                        var novoFilho = novosFilhos.FirstOrDefault(x => comparer.Equals(x, filhoSalvo));
                        if (novoFilho != null)
                        {
                            MesclarFilhos(filhoSalvo, novoFilho);
                        }
                    }
                }

                // Adiciona todos os mesclados no resultSet
                resultSet.AddRange(filhosMesclados);
            }
            else
            {
                if (novosFilhos != null)
                {
                    // Só cairia aqui se não tivesse filhos salvos, porém tivessem novos
                    // O que é improvável, pra não dizer imposssível
                    resultSet.AddRange(novosFilhos);
                }
            }

            return resultSet;
        }

        private void PreencherObjeto(object objeto, IDataReader dr, string alias, List<Join> joins)
        {
            PreencherPais(objeto, dr, alias, joins);
            PreencherFilhos(objeto, dr, joins);
            PreencherCustomFields(objeto, dr, alias);
        }

        private void PreencherPais(object objeto, IDataReader dr, string alias, List<Join> joins)
        {
            Type tipoObjeto = objeto.GetType();

            List<PropertyInfo> props = PegarPropriedadesPais(tipoObjeto);

            foreach (PropertyInfo prop in props)
            {
                int index = PegarIndexNoDataReader($"{alias}{prop.Name}");

                // Valor salvo no banco de dados
                var valor = dr.GetValue(index);

                // Se o valor é nulo ou é DBNull, ignora a propriedade
                if (valor == null || (valor.GetType() == typeof(DBNull))) continue;

                // Se não for um objeto de DBModel
                if (!prop.PropertyType.IsSubclassOf(typeof(IDbModel)))
                {
                    var tipoNaoNulo = prop.NonNullableType();
                    var atributoSerializavel = prop.GetCustomAttribute(typeof(SerializableAttribute));

                    // Verifica o tipo de dados para fazer as conversões
                    if (tipoNaoNulo.IsEnum)
                    {
                        prop.SetValue(objeto, Enum.ToObject(tipoNaoNulo, valor));
                    }
                    else if (atributoSerializavel != null)
                    {
                        var serializado = JsonConvert.DeserializeObject((string)valor, type: tipoNaoNulo);
                        prop.SetValue(objeto, serializado);
                    }
                    else if (tipoNaoNulo == typeof(bool))
                    {
                        prop.SetValue(objeto, Convert.ToBoolean(valor));
                    }
                    else if (tipoNaoNulo == typeof(DateTime))
                    {
                        prop.SetValue(objeto, Convert.ToDateTime(valor));
                    }
                    else
                    {
                        // Se chegou aqui o valor provavelmente é uma string, então não é preciso fazer conversão
                        prop.SetValue(objeto, valor);
                    }

                    // A chave já foi preenchida, então podemos passar para a próxima
                    continue;
                }

                // Caso tenhamos chego aqui sabemos que é uma subclasse de DBModel

                // Verificamos se o join com essa propriedade existe
                // Filtra por classe e por nome, para evitar duplicidade em caso de 2 propriedades do mesmo tipo, porém de nomes diferentes
                Join? join = joins?.Where(join => join.Classe == prop.PropertyType && (prop.Name == join.Campo))?.FirstOrDefault();

                // Se o join não foi feito, criamos o objeto somente com o ID utilizando o implicit Operator
                if (join == null)
                {
                    Type tipo = valor.GetType();
                    var dbModel = (IDbModel)Activator.CreateInstance(prop.PropertyType)!;
                    dbModel.ID = Convert.ToInt64(valor);
                    prop.SetValue(objeto, dbModel);
                    continue;
                }

                // Caso tenhamos chego aqui sabemos que o join foi feito e precisamos utilizar a recursividade para preencher o objeto.
                object subObjeto = Activator.CreateInstance(join!.Classe)!;

                PreencherObjeto(subObjeto, dr, join.Alias!, join.Joins);

                prop.SetValue(objeto, subObjeto);
            }
        }

        private void PreencherFilhos(object objeto, IDataReader dr, List<Join> joins)
        {
            List<PropertyInfo> props = PegarPropriedadesFilhos(objeto.GetType());

            foreach (PropertyInfo prop in props)
            {
                // Como o Children sempre vai ser uma List<T>, sendo T o DBModel, precisamos acessar o tipo de T para comparação
                Type? listGenericType = prop.PropertyType.GetGenericArguments().FirstOrDefault();
                if (listGenericType == null)
                    continue;

                // Tentamos buscar um Join que foi informado
                // especificando o nome da propriedade que
                // deve ser preenchida
                Join? join = joins?.FirstOrDefault(j => 
                    !j.Propriedade.IsNullOrWhiteSpace() && 
                    j.Propriedade == prop.Name && 
                    j.Classe == listGenericType && 
                    j.Relacao == RelacaoTipo.Filho
                );
                            
                // Se não foi encontrado um Join nas condições acima,
                // tentamos buscar o primeiro que bater com a propriedade atual
                join ??= joins?.FirstOrDefault(j => j.Classe == listGenericType && j.Relacao == RelacaoTipo.Filho);
                if (join == null) 
                    continue;

                // Utilizamos um helper para poder utilizar tipo genérico
                MethodInfo helperMethod = typeof(QueryExecutor).GetMethod("PreencherFilhosHelper", BindingFlags.NonPublic | BindingFlags.Instance)!;
                MethodInfo genericHelperMethod = helperMethod.MakeGenericMethod(listGenericType);
                var list = genericHelperMethod.Invoke(this, new object[] { prop.GetValue(objeto)!, dr, join!.Alias!, join.Joins });

                prop.SetValue(objeto, list);
            }
        }

        private void PreencherCustomFields(object objeto, IDataReader dr, string alias)
        {
            var customFields = CustomFieldsCache.ValueIfKeyExists(alias);
            if (customFields.IsNullOrEmpty())
                return;

            foreach (var customField in customFields)
            {
                var valor = dr[customField.CampoAlias];
                if (valor == null || valor.GetType() == typeof(DBNull))
                    continue;

                var propriedade = objeto.GetType().GetProperty(customField.CampoNome);
                if (propriedade == null)
                    continue;

                propriedade.SetValue(objeto, valor);
            }
        }

        private List<T>? PreencherFilhosHelper<T>(List<T>? list, IDataReader dr, string alias, List<Join> joins) where T : IDbModel, new()
        {
            list ??= [];

            int indexId = PegarIndexNoDataReader($"{alias}ID");

            // Conferimos se o ID do objeto é nulo
            // Isso pode acontecer no caso de left join com filho que não tenha registros
            var idObjeto = dr.GetValue(indexId);
            if (idObjeto == null || (idObjeto.GetType() == typeof(DBNull)))
                return list;

            // Aqui estamos lendo 1 linha do DataReader, então só teremos 1 children.
            // Por isso retornamos a lista com esse único elemento dentro e mais tarde mesclamos
            T subObjeto = new ();
            PreencherObjeto(subObjeto, dr, alias, joins);

            list.Add(subObjeto);

            return list;
        }

        private int PegarIndexNoDataReader(string fieldName)
        {
            if (ReferenciaCampos == null)
                throw new Exception("Não foi possível encontrar a referência de campos");
            
            int? index = ReferenciaCampos.ValueIfKeyExists(fieldName);

            if (!index.HasValue)
                throw new Exception("Não foi possível encontrar o Index do campo, erro de persistência!");
            
            return index.Value;
        }

        private List<PropertyInfo> PegarPropriedadesPais(Type tipo)
        {
            string nomeClasse = tipo.FullName!;

            List<PropertyInfo>? props = PropriedadesPaisCache.ValueIfKeyExists(nomeClasse);

            if (props == null)
            {
                props = tipo.Properties(typeof(DBFieldAttribute));

                if (!UtilizarSerializaveis)
                    props = props.Where(prop => prop.GetCustomAttribute(typeof(SerializableAttribute)) == null).ToList();

                PropriedadesPaisCache.Add(nomeClasse, props);
            }

            return props;
        }

        private List<PropertyInfo> PegarPropriedadesFilhos(Type tipo)
        {
            string nomeClasse = tipo.FullName!;

            List<PropertyInfo>? props = PropriedadesFilhoCache.ValueIfKeyExists(nomeClasse);

            if (props == null)
            {
                props = tipo.Properties(typeof(DBChildrenAttribute));
                PropriedadesFilhoCache.Add(nomeClasse, props);
            }

            return props;
        }
    }
}
