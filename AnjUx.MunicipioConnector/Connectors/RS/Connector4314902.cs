using AnjUx.MunicipioConnector.Attributes;
using AnjUx.Shared.Models.Data;
using AnjUx.Shared.Tools;
using Newtonsoft.Json;

namespace AnjUx.MunicipioConnector.Connectors.RS
{
    [MunicipioInfo("4314902", "Porto Alegre", "RS")]
    public class Connector4314902(Municipio municipio) : BaseMunicipioConnector(municipio)
    {
        private readonly string baseUrl = "https://dadosabertos.poa.br/api/3/action/datastore_search?resource_id=88923612-9911-43b1-b6db-27c1dc1a86a1";


		public override async Task<List<MunicipioDado>> GetReceitas(int? ano = null, int? mes = null)
        {
			List<MunicipioDado> pibs = new();

            DateTime hoje = DateTime.Now;

            ano ??= hoje.Year - 1;
            mes ??= hoje.Month;

			var filters = new
			{
				ano,
				mes
			};

            string filtersJSON = JsonConvert.SerializeObject(filters);

			var resultado = await new HttpClientWrapper($"{baseUrl}&filters={Uri.EscapeDataString(filtersJSON)}").Get<ApiResponse>(string.Empty);

            foreach (var registro in resultado.Result?.Records ?? new())
            {
                int registroAno = registro.Ano!.Value;
                int registroMes = registro.Mes!.Value;

                MunicipioDado? pib = pibs.FirstOrDefault(p => p.Ano == registroAno && p.Mes == registroMes);
                if (pib is null)
                {
                    pib = new()
                    {
                        Ano = registroAno,
                        Mes = registroMes,
                        DataBase = new DateTime(registroAno, registroMes, 1, 0, 0, 0),
                        TipoDado = TipoDado.Receita,
                        Valor = 0,
                    };

                    pibs.Add(pib);

				}

                pib.Valor += registro.Valor_Arrecadado;
            }

            return pibs;
        }

        public override Task<List<MunicipioDado>> GetPopulacao(int? ano = null, int? mes = null)
        {
            throw new NotImplementedException();
        }

		private class ApiResponse
        {
            public ApiPIBResponseResult? Result { get; set; }
        }

        private class ApiPIBResponseResult
        {
            public List<ApiPIBResponseResultRecord>? Records { get; set; }
        }

        private class ApiPIBResponseResultRecord
        {
            public int? Ano { get; set; }
            public int? Mes { get; set; }
            public decimal? Valor_Arrecadado { get; set; }
        }
    }
}
