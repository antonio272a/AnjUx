using AnjUx.ORM;
using AnjUx.ORM.Classes;
using AnjUx.Services;
using AnjUx.Shared.Models.Data;

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

        public async Task<List<MunicipioDado>> ListarPorTipoDado(TipoDado? tipo)
        {
            QueryModel<MunicipioDado> query = new();

            Join joinM = new(typeof(Municipio));
            query.AddJoins(joinM);

            if (tipo.HasValue)
                query.Filtros.Add(new Filtro(FiltroTipo.And, OperadorTipo.Igual, query, nameof(MunicipioDado.TipoDado), tipo));

            return await List(query);
        }
    }
}
