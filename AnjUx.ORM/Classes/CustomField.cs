namespace AnjUx.ORM.Classes
{
    public class CustomField
    {
        public string AlvoAlias { get; set; }
        public string CampoAlias { get; set; }
        public string CampoNome { get; set; }
        public string CondicaoPreenchimento { get; set; }

        internal CustomField(string alias, string campoNome, string condicaoPreenchimento)
        {
            AlvoAlias = alias;
            CampoAlias = $"{alias}{campoNome}";
            CampoNome = campoNome;
            CondicaoPreenchimento = condicaoPreenchimento;
        }
    }
}
