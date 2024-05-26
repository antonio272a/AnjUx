using AnjUx.Shared.Models.Data;

namespace AnjUx.MunicipioConnector
{
    public interface IMunicipioConnector
    {
        public Task<List<MunicipioDado>> GetPIB(int? ano = null, int? mes = null);

        public Task<List<MunicipioDado>> GetPopulacao(int? ano = null, int? mes = null);
    }
}
