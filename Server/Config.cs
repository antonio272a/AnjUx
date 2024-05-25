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
            if (stringConexao.IsNullOrWhiteSpace())
                throw new ApplicationException("ConnectionString não encontrada");

            string? baseNome = config.GetSection("BaseNome").Value;
            if (baseNome.IsNullOrWhiteSpace())
                throw new ApplicationException("Nome da Base não encontrado");

            ConnectionString = stringConexao;
            BaseNome = baseNome;
        }

        public string? ConnectionString { get; private set; }
        public string? BaseNome { get; set; }
    }
}
