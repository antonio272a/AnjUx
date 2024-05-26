using AnjUx.MunicipioConnector.Attributes;
using AnjUx.Shared.Models.Data;

namespace AnjUx.MunicipioConnector.RS
{
    [MunicipioInfo("4311403", "Lajeado", "RS")]
    public class Connector4311403 : IMunicipioConnector
    {
        public Task<List<MunicipioDado>> GetPIB()
        {
            throw new NotImplementedException();
        }

        public Task<List<MunicipioDado>> GetPopulacao()
        {
            throw new NotImplementedException();
        }
    }
}
