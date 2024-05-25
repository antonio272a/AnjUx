
namespace AnjUx.ORM.Interfaces
{
    public interface IDBFactoryConsumer : IUserNameDependent
    {
        public DBFactory DBFactory { get; set; }

    }
}
