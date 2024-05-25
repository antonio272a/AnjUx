using AnjUx.Shared.Interfaces;

namespace AnjUx.Server.Interfaces
{
    public interface IBaseService<T> : IBaseService
        where T : IDbModel
    {
        public List<T> Listar();
        public bool Save(T model, bool ignoreIdIntegracaoCheck = false);
        public bool Delete(long? id);
        public T? GetByID(long? id);
    }

    public interface IBaseService : IDisposable
    {
        public string? NomeUsuario { get; set; }
    }
}
