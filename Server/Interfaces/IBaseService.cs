using AnjUx.Shared.Interfaces;

namespace AnjUx.Server.Interfaces
{
    public interface IBaseService<T> : IBaseService
        where T : IDbModel
    {
        public Task<List<T>> ListAll();
        public Task<bool> Save(T model);
        public Task<bool> Delete(long? id);
        public Task<T?> GetByID(long? id);
    }

    public interface IBaseService : IDisposable
    {
        public string? NomeUsuario { get; set; }
    }
}
