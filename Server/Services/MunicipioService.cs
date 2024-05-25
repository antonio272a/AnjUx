using AnjUx.Services;
using AnjUx.Shared.Extensions;
using AnjUx.Shared.Models.Data;
using AnjUx.Shared.Models.Response;

namespace AnjUx.Server.Services
{
    public class MunicipioService : BaseDBService<Municipio>
    {
        public async Task AtualizarMunicipios()
        {
            List<Municipio> municipiosSalvos = await ListAll();

            Dictionary<string, Municipio> dictMunicipios = municipiosSalvos.ToDictionary(m => m.CodigoIBGE!); 

            List<MunicipioIBGE> municipiosIbge = await new IBGEService().GetMunicipios();

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
        }
    }
}
