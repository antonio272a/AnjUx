using AnjUx.MunicipioConnector.Attributes;
using AnjUx.Shared.Models.Data;

namespace AnjUx.MunicipioConnector.RS
{
    [MunicipioInfo("4314902", "Porto Alegre", "RS")]
    public class Connector4314902 : IMunicipioConnector
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
