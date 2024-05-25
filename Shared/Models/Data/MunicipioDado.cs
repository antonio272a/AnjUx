using AnjUx.Shared.Attributes;

namespace AnjUx.Shared.Models.Data
{
    public enum TipoDado : int
    {
        PIB = 0,
        Populacao = 1,
    }

    [DBTable("MunicipioDados")]
    public class MunicipioDado : BaseModel
    {
        #region Fields

        private Municipio? municipio;
        private TipoDado? tipoDado;
        private decimal? valor;
        private DateTime? dataBase;
        private int? mes;
        private int? ano;

        #endregion

        #region

        [DBField()]
        public Municipio? Municipio
        {
            get => municipio;
            set => municipio = value;
        }

        [DBField()]
        public TipoDado? TipoDado
        {
            get => tipoDado;
            set => tipoDado = value;
        }

        [DBField()]
        public decimal? Valor
        {
            get => valor;
            set => valor = value;
        }

        [DBField()]
        public DateTime? DataBase
        {
            get => dataBase;
            set => dataBase = value;
        }

        [DBField()]
        public int? Mes
        {
            get => mes;
            set => mes = value;
        }

        [DBField()]
        public int? Ano
        {
            get => ano;
            set => ano = value;
        }

        #endregion
    }
}
