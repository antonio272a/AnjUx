using AnjUx.Shared.Extensions;
using AnjUx.ORM;
using Dapper;
using System.Reflection;
using Force.DeepCloner;
using AnjUx.ORM.Classes;
using AnjUx.ORM.Interfaces;
using AnjUx.Server;
using AnjUx.Shared.Interfaces;
using AnjUx.Shared.Attributes;
using AnjUx.Shared.Models;
using AnjUx.Server.Interfaces;

namespace AnjUx.Services
{
    public abstract class BaseDBService<T> : IBaseService<T>, IDBFactoryConsumer
        where T : class, IBaseModel
    {
        public BaseDBService(DBFactory? factory = null, string? nomeUsuario = null)
        {

            if (factory == null)
                DBFactory = new DBFactory(Config.Instance.ConnectionString!, "Anônimo");
            else
                DBFactory = factory;

            NomeUsuario = nomeUsuario ?? DBFactory.NomeUsuario;

            var atributoDB = typeof(T).GetCustomAttribute<DBTableAttribute>();
            Table = string.Empty;

            if (atributoDB != null)
            {
                string schemaString = atributoDB!.Schema.IsNullOrWhiteSpace() ? "" : $"{atributoDB!.Schema}.";
                Table = $"{schemaString}{atributoDB!.Table}";
            }

            if (Table.IsNullOrWhiteSpace())
                throw new Exception("Classes derivadas de BaseModel devem utilizar o Atributo \"DBTable\".");
        }

        public DBFactory DBFactory { get; set; }
        public string NomeUsuario { get; set; }
        public string Table { get; set; }

        protected S Resolve<S>() where S : IBaseService
            => (S)Activator.CreateInstance(typeof(S), DBFactory, NomeUsuario)!;

        /// <summary>
        ///   Salva um registro/objeto do banco
        /// </summary>
        /// <param name="objeto">O objeto que será salvo</param>
        protected async Task<bool> Insert(T objeto)
        {
            QueryBuilder queryBuilder = new();

            if (objeto.GetType().GetInterfaces().Any(i => i == typeof(IActiveModel)))
            {
                if (!((IActiveModel)objeto).Active.HasValue)
                    throw new Exception("O objeto que você está tentando salvar não possui o campo Ativo preenchido.");
            }

            if (!queryBuilder.SQLInserir(objeto, Table, DBFactory.NomeUsuario))
                return false;

            DBFactory.NovaTransacao(out bool minhaTransacao);

            // Tenta executar a query de Insert no banco
            try
            {
                await DBFactory.Connection!.ExecuteAsync(queryBuilder.UltimoSQL!, transaction: DBFactory.Transaction);
                var query = $"SELECT * FROM {Table} WHERE ID = @@IDENTITY";
                var objetoSalvo = DBFactory.Connection!.QueryFirst<T>(query, transaction: DBFactory.Transaction);

                // Se o objeto foi salvo corretamente, transferimos seu ID e valores de Insert e Update
                // para o objeto recebido na chamada
                if (objetoSalvo.IsPersisted())
                {
                    objeto.ID = objetoSalvo.ID;
                    objeto.Inserted = objetoSalvo.Inserted;
                    objeto.InsertUser = objetoSalvo.InsertUser;
                    objeto.Updated = objetoSalvo.Updated;
                    objeto.UpdateUser = objetoSalvo.UpdateUser;
                }

                DBFactory.CommitTransacao(minhaTransacao);
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

            return true;
        }

        /// <summary>
        ///   Atualiza um registro/objeto do banco
        /// </summary>
        /// <param name="objeto">Objeto que será atualizado</param>
        protected async Task<bool> Update(T objeto)
        {
            QueryBuilder queryBuilder = new();

            // Instanciamos um novo objeto do tipo T
            var objetoAntigo = Activator.CreateInstance<T>();

            // Passamos para o novo objeto o ID do objeto que está sendo atualizado
            objetoAntigo.ID = objeto.ID;

            // Buscamos no banco a antiga versão do objeto que será atualizado
            if (!(await Open(objetoAntigo)))
                return false;

            if (objetoAntigo.Updated != objeto.Updated)
                throw new Exception("O objeto que você está tentando atualizar já foi alterado por outro usuário. Por favor, atualize a página e tente novamente.");

            // Tentamos criar o SQL
            if (!queryBuilder.SQLAtualizar(objeto, objetoAntigo, Table, DBFactory.NomeUsuario))
                return true;

            DBFactory.NovaTransacao(out bool minhaTransacao);

            try
            {
                await DBFactory.Connection!.ExecuteAsync(queryBuilder.UltimoSQL!, transaction: DBFactory.Transaction);
                var query = $"SELECT * FROM {Table} WHERE ID = {objeto.ID}";
                var objetoAtualizado = await DBFactory.Connection!.QueryFirstAsync<T>(query, transaction: DBFactory.Transaction);

                // Se o objeto foi salvo corretamente, transferimos seus valores de Update
                // para o objeto recebido na chamada
                if (objetoAtualizado.IsPersisted())
                {
                    objeto.Updated = objetoAtualizado.Updated;
                    objeto.UpdateUser = objetoAtualizado.UpdateUser;
                }

                DBFactory.CommitTransacao(minhaTransacao);

                return true;
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

        public async Task<bool> Delete(long? id)
        {
            if (!id.HasValue)
                return false;

            T objeto = Activator.CreateInstance<T>();

            objeto.ID = id;

            if (!(await Open(objeto)))
                return false;

            return await Delete(objeto);
        }

        /// <summary>
        ///   Exclui um registro/objeto do banco mediante passagem de ID do objeto
        /// </summary>
        /// <param name="objeto">ID do objeto que será excluído</param>
        public async Task<bool> Delete(T objeto)
        {
            if (!objeto.IsPersisted())
                return false;

            var query = $"DELETE FROM {Table} WHERE ID = {objeto.ID}";

            DBFactory.NovaTransacao(out bool minhaTransacao);

            try
            {
                await DBFactory.Connection!.ExecuteAsync(query, transaction: DBFactory.Transaction);

                DBFactory.CommitTransacao(minhaTransacao);

                return true;
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

        /// <summary>
        ///   Lê um registro/objeto do banco
        /// </summary>
        /// <param name="objeto">ID do objeto que será lido do banco</param>
        /// <returns></returns>
        public async Task<bool> Open(T objeto)
        {
            // Query padrão para buscar um registro no banco
            var sql = $"SELECT * FROM {Table} WHERE ID = {objeto.ID}";

            DBFactory.NovaTransacao(out bool minhaTransacao);

            try
            {
                var encontrato = await DBFactory.Connection!.QueryFirstOrDefaultAsync<T>(sql, transaction: DBFactory.Transaction);

                DBFactory.CommitTransacao(minhaTransacao);

                if (encontrato == null)
                    return false;

                encontrato.DeepCloneTo(objeto);

                return true;
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

        public async Task<List<T>> ListAll()
        {
            QueryModel<T> model = new();
            return await List(model);
        }

        public async Task<List<T>> List(QueryModel<T> model)
        {
            QueryExecutor executor = new(this);
            return await executor.Listar(model);
        }

        public async Task<bool> Open(T objeto, QueryModel<T> model)
        {
            QueryExecutor executor = new(this);
            return await executor.AbrirComJoins(objeto, model);
        }

        public virtual async Task<bool> Save(T objeto)
        {
            if (objeto.IsPersisted())
            {
                return await Update(objeto);
            }
            else
            {
                return await Insert(objeto);
            }
        }

        public virtual async Task<bool> Swizzle(T objeto)
        {
            if (!objeto.IsPersisted() || !objeto.Updated.HasValue)
                return await Open(objeto);

            return true;
        }

        public virtual void Validate(T objeto)
        {
            if (objeto == null)
                throw new Exception($"Não é possível salvar no banco um objeto nulo.");
        }

        public virtual void Dispose()
        {
            DBFactory.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task<T?> GetByID(long? id)
        {
            QueryModel<T> model = new("T");

            model.Filtros.Add(new Filtro(FiltroTipo.And, OperadorTipo.Igual, "T.ID", id));

            return (await List(model)).FirstOrDefault();
        }
    }
}
