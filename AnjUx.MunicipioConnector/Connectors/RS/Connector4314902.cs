using AnjUx.MunicipioConnector.Attributes;
using AnjUx.Shared.Models.Data;

namespace AnjUx.MunicipioConnector.Connectors.RS
{
    [MunicipioInfo("4314902", "Porto Alegre", "RS")]
    public class Connector4314902 : IMunicipioConnector
    {
        public Task<List<MunicipioDado>> GetReceitas(int? ano = null, int? mes = null)
        {
            throw new NotImplementedException();
        }

        public Task<List<MunicipioDado>> GetPopulacao(int? ano = null, int? mes = null)
        {
            throw new NotImplementedException();
        }
    }
}
