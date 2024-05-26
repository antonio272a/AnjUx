

namespace AnjUx.MunicipioConnector.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MunicipioInfoAttribute(string codigoIBGE, string nome, string uf) : Attribute
    {
        public string CodigoIBGE { get; set; } = codigoIBGE;
        public string Nome { get; set; } = nome;
        public string UF { get; set; } = uf;
    }
}
