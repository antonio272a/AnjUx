﻿using AnjUx.Client.Services;
using AnjUx.Shared.Extensions;
using AnjUx.ORM;
using Dapper;
using System.Reflection;
using AnjUx.ORM.Attributes;
using Force.DeepCloner;
using AnjUx.ORM.Classes;
using AnjUx.ORM.Interfaces;
using AnjUx.Server;
using AnjUx.Shared.Interfaces;

namespace AnjUx.Services
{
    public abstract class BaseDBService<T> : IBaseService, IDBFactoryConsumer
        where T : class, IBaseModel
    {
        public BaseDBService(LoadingService loadingService, CoreNotificationService notificationService, DBFactory? factory = null)
        {
            LoadingService = loadingService;
            NotificationService = notificationService;

            if (factory == null)
                DBFactory = new DBFactory(Config.Instance.ConnectionString!, "Anônimo");
            else
                DBFactory = factory;

            NomeUsuario = DBFactory.NomeUsuario;

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

        public LoadingService LoadingService { get; set; }
        public CoreNotificationService NotificationService { get; set; }
        public DBFactory DBFactory { get; set; }
        public string NomeUsuario { get; set; }
        public string Table { get; set; }

        protected async Task<X> RunWithLoading<X>(Func<Task<X>> func)
        {
            try
            {
                LoadingService.IsLoading = true;
                return await func.Invoke();
            }
            catch (Exception ex)
            {
                throw ex.ReThrow();
            }
            finally
            {
                LoadingService.IsLoading = false;
            }
        }

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

            return await RunWithLoading(async () =>
            {
                DBFactory.NovaTransacao(out bool minhaTransacao);

                // Tenta executar a query de Insert no banco
                try
                {
                    await DBFactory.Connection!.ExecuteAsync(queryBuilder.UltimoSQL!, transaction: DBFactory.Transaction);
                    var query = $"SELECT * FROM {Table} WHERE ID = last_insert_rowid();";
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
            });
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

            return await RunWithLoading(async () => {
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
            });
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

            return await RunWithLoading(async () => {
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
            });
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

            return await RunWithLoading(async () =>
            {
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
            });
        }

        public async Task<List<T>> ListAll()
        {
            QueryModel<T> model = new();
            return await List(model);
        }

        public async Task<List<T>> List(QueryModel<T> model)
        {
            QueryExecutor executor = new(this);
            return await RunWithLoading(async () => await executor.Listar(model));
        }

        public async Task<bool> Open(T objeto, QueryModel<T> model)
        {
            QueryExecutor executor = new(this);
            return await RunWithLoading(async () => await executor.AbrirComJoins(objeto, model));
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
    }
}
