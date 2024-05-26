using AnjUx.Shared.Attributes;

namespace AnjUx.Shared.Models.Data
{
    public enum TarefaStatus : int
    {
        Aberta = 0,
        Finalizada = 1,
        Erro = 2,
    }

    [DBTable("Tarefas")]
    public class Tarefa : BaseModel
    {
        public static implicit operator Tarefa(long? id) => new() { ID = id };
        public static implicit operator Tarefa(long id) => new() { ID = id };

        #region Fields

        private string? descricao;
        private TarefaStatus? status;
        private DateTime? finalizada;
        private string? erro;

        #endregion

        #region Properties

        [DBField()]
        public string? Descricao
        {
            get => descricao;
            set => descricao = value;
        }

        [DBField()]
        public TarefaStatus? Status
        {
            get => status;
            set => status = value;
        }

        [DBField()]
        public DateTime? Finalizada
        {
            get => finalizada;
            set => finalizada = value;
        }

        [DBField()]
        public string? Erro
        {
            get => erro;
            set => erro = value;
        }

        #endregion
    }
}
