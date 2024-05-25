

namespace AnjUx.Shared.Models.Response
{
    public class MunicipioIBGE
    {
        public string? ID { get; set; }
        public string? Nome { get; set; }
        public MicrorregiaoIBGE? Microrregiao { get; set; }
    }

    public class MicrorregiaoIBGE
    {
        public MesorregiaoIBGE? Mesorregiao { get; set; }
    }

    public class MesorregiaoIBGE
    {
        public UFIBGE? UF { get; set; }
    }

    public class UFIBGE
    {
        public string? Sigla { get; set; }
    }
}
