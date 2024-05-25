using AnjUx.Shared.Extensions;

namespace AnjUx.Server
{
    public class Config
    {
        private static Config? instance;

        public static Config Instance
        {
            get 
            {
                instance ??= new Config();
                return instance;
            }
        }

        private Config()
        {
        }

        public void Initialize(IConfiguration config)
        {
            string? stringConexao = config.GetSection("ConnectionString").Value;
            if (stringConexao.IsNullOrWhiteSpace())
                throw new ApplicationException("ConnectionString não encontrada");

            string? dbName = config.GetSection("DbName").Value;
            if (dbName.IsNullOrWhiteSpace())
                throw new ApplicationException("Nome da Base não encontrado");

            ConnectionString = stringConexao;
            DbName = dbName;
        }

        public string? ConnectionString { get; private set; }
        public string? DbName { get; set; }
    }
}
