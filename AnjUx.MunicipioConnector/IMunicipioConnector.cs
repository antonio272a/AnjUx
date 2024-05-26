using AnjUx.Shared.Models.Data;

namespace AnjUx.MunicipioConnector
{
    public interface IMunicipioConnector
    {
        public Task<List<MunicipioDado>> GetPIB();

        public Task<List<MunicipioDado>> GetPopulacao();
    }
}
