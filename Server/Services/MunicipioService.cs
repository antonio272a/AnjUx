using AnjUx.MunicipioConnector;
using AnjUx.ORM;
using AnjUx.ORM.Classes;
using AnjUx.Services;
using AnjUx.Shared.Extensions;
using AnjUx.Shared.Models.Data;
using AnjUx.Shared.Models.Response;
using System.Reflection;

namespace AnjUx.Server.Services
{
    public class MunicipioService(DBFactory? factory = null, string? nomeUsuario = null) : BaseDBService<Municipio>(factory, nomeUsuario)
    {
        public async Task<List<Municipio>> Buscar(string? termo)
        {
            QueryModel<Municipio> query = new("M");

            if (termo.IsNotNullOrWhiteSpace())
                query.Filtros.Add(new Filtro(FiltroTipo.And, OperadorTipo.Like, query, nameof(Municipio.Nome), termo));

            return await List(query);
        }

        public async Task<List<Municipio>> Listar(int? top)
        {
            QueryModel<Municipio> query = new("M");

            query.Top = top;

            return await List(query);
        }

        public async Task BuscarInformacoes(long? id)
        {
            if (!id.HasValue)
                throw new Exception("Município não informado!");

            Municipio? municipio = await GetByID(id);

            if (!municipio.IsPersisted())
                throw new Exception("Município não encontrado!");

            string nomeConector = $"Connector{municipio.CodigoIBGE}";

            Type tipoConector = typeof(IMunicipioConnector).Assembly.GetType($"AnjUx.MunicipioConnector.Connectors.{municipio.UF}.{nomeConector}") ?? throw new Exception("Conector não encontrado!");
        
            IMunicipioConnector conector = (IMunicipioConnector)Activator.CreateInstance(tipoConector)!;

            await conector.GetPIB();
        }

        public async Task AtualizarMunicipios()
        {
            List<Municipio> municipiosSalvos = await ListAll();

            Dictionary<string, Municipio> dictMunicipios = municipiosSalvos.ToDictionary(m => m.CodigoIBGE!); 

            List<MunicipioIBGE> municipiosIbge = await new IBGEService().GetMunicipios();

            DBFactory!.NovaTransacao(out bool minhaTransacao);
            
            try
            {
                foreach (MunicipioIBGE municipioIbge in municipiosIbge)
                {
                    Municipio? municipio = dictMunicipios!.ValueIfKeyExists(municipioIbge.ID);

                    if (municipio.IsPersisted())
                        // Verifica se alguma propriedade mudou, caso nenhuma tenha mudado, continue
                        if (
                            municipio.Nome == municipioIbge.Nome &&
                            municipio.UF == municipioIbge.Microrregiao?.Mesorregiao?.UF?.Sigla
                            )
                            continue;

                    municipio ??= new Municipio();

                    municipio.CodigoIBGE = municipioIbge.ID;
                    municipio.Nome = municipioIbge.Nome;
                    municipio.UF = municipioIbge.Microrregiao?.Mesorregiao?.UF?.Sigla;

                    await Save(municipio);
                }

                DBFactory.CommitTransacao(minhaTransacao);
            }
            catch (Exception ex)
            {
                DBFactory.RollbackTransacao(minhaTransacao);
                throw ex.ReThrow();
            }
            finally
            {
                DBFactory.FecharConexao(minhaTransacao);
            }
        }
    }
}
