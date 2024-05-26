using AnjUx.Shared.Extensions;
using AnjUx.Shared.Models.Data;

namespace AnjUx.MunicipioConnector
{
	public abstract class BaseMunicipioConnector(Municipio municipio) : IMunicipioConnector
	{
		protected Municipio Municipio => municipio;
        protected virtual List<int> _meses { get; set; } = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];
        protected virtual int _anoInicial { get; set; } = 2018;

        public virtual async Task<List<MunicipioDado>> GetPopulacao(int? ano = null, int? mes = null)
		{
			IBGEScraper scrapper = new();

            Dictionary<int, Dictionary<string, MunicipioDado>> resultado = await scrapper.BuscarEstimativarPopulacionais(_anoInicial, DateTime.Now.Year, municipio.CodigoIBGE);

			List<MunicipioDado> dados = [];

			foreach (int anoResultado in resultado.Keys)
				if (resultado[anoResultado]!.TryGetValue(municipio.CodigoIBGE!, out MunicipioDado? value))
					dados.Add(value);

			return dados;
		}

		public abstract Task<List<MunicipioDado>> GetReceitas(int? ano = null, int? mes = null);
	}
}
