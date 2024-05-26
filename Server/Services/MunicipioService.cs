using AnjUx.MunicipioConnector;
using AnjUx.ORM;
using AnjUx.ORM.Classes;
using AnjUx.Services;
using AnjUx.Shared.Extensions;
using AnjUx.Shared.Models.Data;
using AnjUx.Shared.Models.Response;

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

        public async Task<List<MunicipioDado>> BuscarReceitas(long? id)
        {
            var tarefaService = Resolve<TarefaService>();
            var municipioDadoService = Resolve<MunicipioDadoService>();

            Tarefa tarefa = await tarefaService.NovaTarefa($"Buscar Receitas município \"{id}\"");

            try
            {
                if (!id.HasValue)
                    throw new Exception("Município não informado!");

                Municipio? municipio = await GetByID(id);

                if (!municipio.IsPersisted())
                    throw new Exception("Município não encontrado!");

                tarefa.Descricao = $"Buscar Receitas município \"{municipio.CodigoIBGE} - {municipio.Nome}\"";
                await tarefaService.Save(tarefa);

                #region Buscar Conector

                string nomeConector = $"Connector{municipio.CodigoIBGE}";

                Type tipoConector = typeof(IMunicipioConnector).Assembly.GetType($"AnjUx.MunicipioConnector.Connectors.{municipio.UF}.{nomeConector}") ?? throw new Exception("Conector não encontrado!");
        
                IMunicipioConnector conector = (IMunicipioConnector)Activator.CreateInstance(tipoConector, municipio)!;

                #endregion

                #region Buscar Dados

                List<MunicipioDado> dadosSalvos = await municipioDadoService.ListarPorMunicipio(id, TipoDado.Receita);

                Dictionary<string, MunicipioDado> referencia = dadosSalvos.ToDictionary(d => $"{d.Ano}-{d.Mes}");

                List<MunicipioDado> dados = await conector.GetReceitas();

                #endregion

                #region Salvar Dados

                DBFactory.NovaTransacao(out bool minhaTransacao);

                try
                {
                    foreach (MunicipioDado dado in dados)
                    {
                        string chave = $"{dado.Ano}-{dado.Mes}";

                        if (referencia!.TryGetValue(chave, out MunicipioDado? dadoSalvo))
                        {
                            if (dadoSalvo.Valor == dado.Valor)
                                continue;

                            dadoSalvo.Valor = dado.Valor;
                            dadoSalvo.Fonte = dado.Fonte ?? "Conector Próprio";
                            await municipioDadoService.Save(dadoSalvo);
                        }
                        else
                        {
                            dado.Municipio = municipio;
                            dado.TipoDado = TipoDado.Receita;
                            dado.Fonte ??= "Conector Próprio";
                            await municipioDadoService.Save(dado);
                        }
                    }

                    DBFactory.CommitTransacao(minhaTransacao);
                }
                catch (Exception ex)
                {
                    DBFactory.RollbackTransacao(minhaTransacao);
                    throw ex.ReThrow();
                }

                #endregion

                await tarefaService.FinalizarTarefa(tarefa);

                return dados;
            }
            catch (Exception ex)
            {
                tarefa.Erro = ex.Message;
                tarefa.Status = TarefaStatus.Erro;
                await tarefaService.Save(tarefa);

                throw ex.ReThrow();
            }
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
