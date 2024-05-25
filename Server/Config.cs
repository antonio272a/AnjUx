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

        public void Initialize(IConfigurationSection config)
        {
            string? stringConexao = config.GetSection("ConnectionString").Value;

            if(stringConexao.IsNullOrWhiteSpace())
                throw new ApplicationException("ConnectionString não encontrada");

            ConnectionString = stringConexao;
        }

        public string? ConnectionString { get; private set; }
    }
}
