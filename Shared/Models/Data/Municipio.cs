using AnjUx.Shared.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnjUx.Shared.Models.Data
{
    [DBTable("Municipios")]
    public class Municipio : BaseModel
    {
        public static implicit operator Municipio(long? id) => new() { ID = id };
        public static implicit operator Municipio(long id) => new() { ID = id };

        #region Fields

        private string? nome;
        private string? uf;
        private string? codigoIBGE;

        #endregion

        #region Properties

        [DBField()]
        public string? Nome
        {
            get => nome;
            set => nome = value;
        }

        [DBField()]
        public string? UF
        {
            get => uf;
            set => uf = value;
        }

        [DBField()]
        [Display(Name = "Código IBGE")]
        public string? CodigoIBGE
        {
            get => codigoIBGE;
            set => codigoIBGE = value;
        }

        #endregion

        #region Virtuals

        public string FullName => $"{CodigoIBGE} - {Nome}";

        #endregion
    }
}
