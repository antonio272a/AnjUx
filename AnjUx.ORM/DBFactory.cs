using Microsoft.Data.Sqlite;
using AnjUx.ORM.Interfaces;
using System.Data;
using Microsoft.Data.SqlClient;

namespace AnjUx.ORM
{
    public class DBFactory : IUserNameDependent, IDisposable
    {
        public string NomeUsuario { get; private set; }
        public string ConnectionString { get; private set; }
        public IDbConnection? Connection { get; private set; }
        public IDbTransaction? Transaction { get; private set; }

        public bool InTransaction => Transaction != null;

        public DBFactory(string connectionString, string? userName = null)
        {
            NomeUsuario = userName ?? "Anônimo";
            ConnectionString = connectionString;
        }

        public void NovaTransacao(out bool minhaTransacao)
        {
            minhaTransacao = false;

            if (InTransaction)
                return;

            Connection = new SqlConnection(ConnectionString);
            Connection.Open();
            Transaction = Connection.BeginTransaction();

            minhaTransacao = true;
        }

        public void RollbackTransacao(bool minhaTransacao)
        {
            if (minhaTransacao && InTransaction)
                Transaction!.Rollback();
        }

        public void CommitTransacao(bool minhaTransacao)
        {
            if (!minhaTransacao || !InTransaction)
                return;

            Transaction!.Commit();
            Transaction!.Dispose();
            Transaction = null;
        }

        public void FecharConexao(bool minhaTransacao)
        {
            if (!minhaTransacao || Connection == null)
                return;

            Connection!.Close();
            Connection!.Dispose();
            Connection = null;
        }

        public void Dispose()
        {
            if (InTransaction)
                RollbackTransacao(true);

            FecharConexao(true);
            GC.SuppressFinalize(this);
        }
    }
}
