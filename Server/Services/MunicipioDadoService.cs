using AnjUx.ORM;
using AnjUx.ORM.Classes;
using AnjUx.Services;
using AnjUx.Shared.Extensions;
using AnjUx.Shared.Models.Data;
using AnjUx.Shared.Models.Requests;
using AnjUx.Shared.Models.Response;

namespace AnjUx.Server.Services
{
    public class MunicipioDadoService(DBFactory? factory = null, string? nomeUsuario = null) : BaseDBService<MunicipioDado>(factory, nomeUsuario)
    {
        public async Task<List<MunicipioDado>> ListarPorMunicipio(long? idMunicipio, TipoDado? tipo = null)
        {
            QueryModel<MunicipioDado> query = new();

            if (!idMunicipio.HasValue || idMunicipio.Value <= 0)
                throw new InvalidOperationException("Informe o Município");

            query.Filtros.Add(new Filtro(FiltroTipo.And, OperadorTipo.Igual, query, nameof(MunicipioDado.Municipio), idMunicipio));

            if (tipo.HasValue)
                query.Filtros.Add(new Filtro(FiltroTipo.And, OperadorTipo.Igual, query, nameof(MunicipioDado.TipoDado), tipo));

            return await List(query);
        }

        public async Task<List<MunicipioDado>> ListarPorTipoDado(TipoDado? tipo, int? ano = null, int? mes = null, List<Municipio>? municipios = null)
        {
            QueryModel<MunicipioDado> query = new();

            Join joinM = new(typeof(Municipio));
            query.AddJoins(joinM);

            if (tipo.HasValue)
                query.Filtros.Add(new Filtro(FiltroTipo.And, OperadorTipo.Igual, query, nameof(MunicipioDado.TipoDado), tipo));

            if (ano.HasValue)
                query.Filtros.Add(new Filtro(FiltroTipo.And, OperadorTipo.Igual, query, nameof(MunicipioDado.Ano), ano));

            if (mes.HasValue)
                query.Filtros.Add(new Filtro(FiltroTipo.And, OperadorTipo.Igual, query, nameof(MunicipioDado.Mes), mes));

            if (!municipios.IsNullOrEmpty())
                query.Filtros.Add(new Filtro(FiltroTipo.And, OperadorTipo.Em, query, nameof(MunicipioDado.Municipio), municipios.Select(x => x.ID).ToCommaString()));

            return await List(query);
        }

        public async Task<List<MunicipioDado>> GetPopulacao(int ano, List<Municipio>? municipios)
        {
            QueryModel<MunicipioDado> query = new("MD");

            Join joinM = new(typeof(Municipio));
            query.AddJoins(joinM);

            query.Filtros.Add(new Filtro(FiltroTipo.And, OperadorTipo.Igual, query, nameof(MunicipioDado.TipoDado), TipoDado.Populacao));

            if (!municipios.IsNullOrEmpty())
                query.Filtros.Add(new Filtro(FiltroTipo.And, OperadorTipo.Em, query, nameof(MunicipioDado.Municipio), municipios.Select(x => x.ID).ToCommaString()));

            // Como podemos não ter dados de população precisos no ano, então trazemos o mais recente
            query.Filtros.Add(
                new Filtro(
                    $@"
                    AND MD.ID IN (
                        SELECT MAX(MD2.ID) 
	                    FROM MunicipioDados MD2
	                    WHERE 1 = 1
                        AND MD2.TipoDado = {(int)TipoDado.Populacao}
                        AND MD2.Ano <= {ano}
	                    GROUP BY MD2.Municipio
                    )"
                    ));

            return await List(query);
        }

        public async Task<ComparadorModel> Comparador(RequestComparador request)
        {
            int anoInicial = request.AnoInicial ?? throw new ArgumentException("Informe o Ano Inicial");
            int anoFinal = request.AnoFinal ?? DateTime.Now.Year;

            int mesInicial = (int?)request.MesInicial ?? 1;
            int mesFinal = (int?)request.MesFinal ?? DateTime.Now.Month;

            DateTime dataInicial = new(anoInicial, mesInicial, 1);
            DateTime dataFinal = new(anoFinal, mesFinal, 1);

            if (dataInicial > dataFinal)
                throw new ArgumentException("Data Inicial não pode ser maior que a Data Final");

            ComparadorModel response = new()
            {
                AnoFinal = anoFinal,
                AnoInicial = anoInicial,
                MesFinal = mesFinal,
                MesInicial = mesInicial,
                Municipios = []
            };

            List<MunicipioDado> receitasInicial = await ListarPorTipoDado(TipoDado.Receita, anoInicial, mesInicial, request.Municipios);
            if (receitasInicial.IsNullOrEmpty())
                throw new InvalidOperationException($"Não foram encontrados dados de Receita para o Ano \"{anoInicial}\" e Mês \"{mesFinal}\"");

            List<MunicipioDado> receitasFinal = await ListarPorTipoDado(TipoDado.Receita, anoFinal, mesFinal, request.Municipios);
            if (receitasFinal.IsNullOrEmpty())
                throw new InvalidOperationException($"Não foram encontrados dados de Receita para o Ano \"{anoFinal}\" e Mês \"{mesFinal}\"");

            List<MunicipioDado> populacaoInicial = await GetPopulacao(anoInicial, request.Municipios);
            if (populacaoInicial.IsNullOrEmpty())
            throw new InvalidOperationException($"Não foram encontrados dados de População para o Ano \"{anoInicial}\"");

            List<MunicipioDado> populacaoFinal = await GetPopulacao(anoFinal, request.Municipios);
            if (populacaoFinal.IsNullOrEmpty())
                throw new InvalidOperationException($"Não foram encontrados dados de População para o Ano \"{anoFinal}\"");
            
            Dictionary<long, MunicipioDado> receitasInicialDict = receitasInicial.ToDictionary(x => x.Municipio!.ID!.Value);
            Dictionary<long, MunicipioDado> receitasFinalDict = receitasFinal.ToDictionary(x => x.Municipio!.ID!.Value);
            Dictionary<long, MunicipioDado> populacaoInicialDict = populacaoInicial.ToDictionary(x => x.Municipio!.ID!.Value);
            Dictionary<long, MunicipioDado> populacaoFinalDict = populacaoFinal.ToDictionary(x => x.Municipio!.ID!.Value);

            List<long> idsMunicipios = receitasInicialDict.Keys
            .Intersect(receitasFinalDict.Keys)
            .Intersect(populacaoInicialDict.Keys)
            .Intersect(populacaoFinalDict.Keys)
            .ToList();

            foreach (long idMunicipio in idsMunicipios)
            {
                response.Municipios.Add(new()
                {
                    Municipio = receitasInicialDict[idMunicipio].Municipio,
                    DadosIniciais =
                    [
                        receitasInicialDict[idMunicipio],
                        populacaoInicialDict[idMunicipio]
                    ],
                    DadosFinais =
                    [
                        receitasFinalDict[idMunicipio],
                        populacaoFinalDict[idMunicipio]
                    ]
                });
            }

            return response;
        }

    }
}
