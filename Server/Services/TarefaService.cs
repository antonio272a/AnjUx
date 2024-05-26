using AnjUx.ORM;
using AnjUx.Services;
using AnjUx.Shared.Extensions;
using AnjUx.Shared.Models.Data;
using Dapper;

namespace AnjUx.Server.Services
{
    public class TarefaService(DBFactory? factory = null, string? nomeUsuario = null) : BaseDBService<Tarefa>(factory, nomeUsuario)
    {
        public async Task<Tarefa> NovaTarefa(string descricao)
        {
            Tarefa tarefa = new()
            {
                Descricao = descricao,
                Status = TarefaStatus.Aberta
            };

            await Save(tarefa);

            return tarefa;
        }

        public async Task<bool> FinalizarTarefa(Tarefa tarefa)
        {
            if (tarefa.Finalizada != null)
                throw new InvalidOperationException("Tarefa já finalizada");

            tarefa.Finalizada = DateTime.Now;
            tarefa.Status = TarefaStatus.Finalizada;

            return await Save(tarefa);
        }

        public async Task FalharTarefa(Tarefa tarefa, Exception ex)
        {
            tarefa.Erro = ex.Message;
            tarefa.Status = TarefaStatus.Erro;
            await Save(tarefa);
        }

        public override void Validate(Tarefa objeto)
        {
            if (objeto.Status == null)
                throw new InvalidOperationException("Informe o Status da Tarefa");

            base.Validate(objeto);
        }

        public async Task<bool> Limpar()
        {
            DBFactory.NovaTransacao(out bool minhaTransacao);
            try
            {
                string sql = "DELETE FROM Tarefas";

                await DBFactory.Connection!.ExecuteAsync(sql, transaction: DBFactory.Transaction);

                DBFactory.CommitTransacao(minhaTransacao);

                return true;
            }
            catch (Exception ex)
            {
                DBFactory.RollbackTransacao(minhaTransacao);
                throw ex.ReThrow();
            }
        }
    }
}
