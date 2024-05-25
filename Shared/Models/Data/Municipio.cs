using AnjUx.Shared.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnjUx.Shared.Models.Data
{
    [DBTable("Municipios")]
    public class Municipio : BaseModel
    {
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
        public string? CodigoIBGE
        {
            get => codigoIBGE;
            set => codigoIBGE = value;
        }

        #endregion
    }
}
