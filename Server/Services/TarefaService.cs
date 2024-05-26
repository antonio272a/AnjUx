using AnjUx.Services;
using AnjUx.Shared.Models.Data;

namespace AnjUx.Server.Services
{
    public class TarefaService : BaseDBService<Tarefa>
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

        public override void Validate(Tarefa objeto)
        {
            if (objeto.Status == null)
                throw new InvalidOperationException("Informe o Status da Tarefa");

            base.Validate(objeto);
        }
    }
}
