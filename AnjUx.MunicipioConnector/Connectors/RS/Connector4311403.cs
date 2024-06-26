﻿using AnjUx.MunicipioConnector.Attributes;
using AnjUx.Shared.Models.Data;
using AnjUx.Shared.Tools;

namespace AnjUx.MunicipioConnector.Connectors.RS
{
    [MunicipioInfo("4311403", "Lajeado", "RS")]
    public class Connector4311403(Municipio municipio) : BaseMunicipioConnector(municipio)
    {
        private readonly string _baseUrl = "https://grp.lajeado.rs.gov.br/infra/apigw/transparencia/service/contabilidade/transparencia/receita/receitaArrecadada";

        public override async Task<List<MunicipioDado>> GetReceitas(int? ano = null, int? mes = null)
        {
            List<MunicipioDado> resultado = [];

            if (ano != null && mes != null)
                return [await GetReceitasInternal(ano.Value, mes.Value)];

            if (mes != null)
                throw new InvalidOperationException("Não é possível informar somente o mês!");

            if (ano != null)
            {
                bool anoAtual = ano == DateTime.Now.Year;
                List<int> meses = anoAtual ? _meses.Where(m => m <= DateTime.Now.Month).ToList() : _meses;

                foreach (int _mes in _meses)
                    resultado.Add(await GetReceitasInternal(ano.Value, _mes));
            }
            else
            {
                for (int _ano = _anoInicial; _ano <= DateTime.Now.Year; _ano++)
                {
                    bool anoAtual = _ano == DateTime.Now.Year;
                    List<int> meses = anoAtual ? _meses.Where(m => m <= DateTime.Now.Month).ToList() : _meses;

                    foreach (int _mes in meses)
                        resultado.Add(await GetReceitasInternal(_ano, _mes));

                }
            }

            return resultado;
        }

        public async Task<MunicipioDado> GetReceitasInternal(int ano, int mes)
        {
            HttpClientWrapper client = new(_baseUrl);

            BuscaPIBRequest request = new()
            {
                Exercicio = ano,
                Mes = mes.ToString().PadLeft(2, '0')
            };

            BuscaPIBResponse response = await client.Post<BuscaPIBRequest, BuscaPIBResponse>("", request);

            // Agora precisamos filtrar todos os resultados de plano de contas raiz, ex.: 1.0.0.0.00.00.00 e 2.0.0.00.0.00.00
            List<Receita>? receitas = response.Resultado?.Where(r => r.CodigoFormatado?.Replace("0", "").Replace(".", "").Length == 1).ToList();

            decimal valor = receitas?.Sum(r => r.ValorArrecadadoMensal) ?? 0;

            MunicipioDado dado = new()
            {
                Municipio = Municipio,
                Valor = valor,
                TipoDado = TipoDado.Receita,
                Ano = ano,
                Mes = mes,
                Data = new DateTime(ano, mes, 1, 0, 0, 0),
                Fonte = "Portal da Transparência Lajeado"
            };

            return dado;
        }

        private class BuscaPIBRequest
        {
            public int? Exercicio { get; set; } // ano
            public string? Mes { get; set; }
        }

        private class BuscaPIBResponse
        {
            public List<Receita>? Resultado { get; set; }
        }

        private class Receita
        {
            public string? CodigoFormatado { get; set; }
            public decimal? ValorArrecadado { get; set; }
            public decimal? ValorArrecadadoMensal { get; set; }
        }
    }
}
