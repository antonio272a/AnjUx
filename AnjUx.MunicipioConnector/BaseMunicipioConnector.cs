using AnjUx.Shared.Models.Data;

namespace AnjUx.MunicipioConnector
{
	public abstract class BaseMunicipioConnector(Municipio municipio) : IMunicipioConnector
	{
		protected Municipio Municipio => municipio;

		public abstract Task<List<MunicipioDado>> GetPopulacao(int? ano = null, int? mes = null);

		public abstract Task<List<MunicipioDado>> GetReceitas(int? ano = null, int? mes = null);
	}
}
