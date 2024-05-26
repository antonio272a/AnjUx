using AnjUx.MunicipioConnector.Attributes;
using AnjUx.Shared.Models.Data;

namespace AnjUx.MunicipioConnector.RS
{
    [MunicipioInfo("4311403", "Lajeado", "RS")]
    public class Connector4311403 : IMunicipioConnector
    {
        public Task<List<MunicipioDado>> GetPIB(int? ano = null, int? mes = null)
        {
            throw new NotImplementedException();
        }

        public Task<List<MunicipioDado>> GetPopulacao(int? ano = null, int? mes = null)
        {
            throw new NotImplementedException();
        }
    }
}
