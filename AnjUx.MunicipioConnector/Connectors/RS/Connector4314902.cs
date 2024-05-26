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
            List<MunicipioDado> resultado = [];

            if (ano != null && mes != null)
                return [.. await GetReceitasInternal(ano.Value, mes.Value)];

            if (mes != null)
                throw new InvalidOperationException("Não é possível informar somente o mês!");

            if (ano != null)
            {
                bool anoAtual = ano == DateTime.Now.Year;
                List<int> meses = anoAtual ? _meses.Where(m => m <= DateTime.Now.Month).ToList() : _meses;

                foreach (int _mes in _meses)
                    resultado.AddRange(await GetReceitasInternal(ano.Value, _mes));
            }
            else
            {
                for (int _ano = _anoInicial; _ano <= DateTime.Now.Year; _ano++)
                {
                    bool anoAtual = _ano == DateTime.Now.Year;
                    List<int> meses = anoAtual ? _meses.Where(m => m <= DateTime.Now.Month).ToList() : _meses;

                    foreach (int _mes in meses)
                        resultado.AddRange(await GetReceitasInternal(_ano, _mes));

                }
            }

            return resultado;
        }

        private async Task<List<MunicipioDado>> GetReceitasInternal(int ano, int mes)
        {
            List<MunicipioDado> pibs = [];

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
                        Municipio = Municipio,
                        Ano = registroAno,
                        Mes = registroMes,
                        Data = new DateTime(registroAno, registroMes, 1, 0, 0, 0),
                        TipoDado = TipoDado.Receita,
                        Valor = 0,
                        Fonte = "API Porto Alegre"
                    };

                    pibs.Add(pib);

                }

                pib.Valor += registro.Valor_Arrecadado;
            }

            return pibs;
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
