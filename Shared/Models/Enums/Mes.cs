using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AnjUx.Shared.Models.Enums
{
    public enum Mes : int
    {
        [Display(AutoGenerateField = false)]
        None = 0,

        Janeiro = 1,
        Fevereiro = 2,
        [Description("Março")]
        Marco = 3,
        Abril = 4,
        Maio = 5,
        Junho = 6,
        Julho = 7,
        Agosto = 8,
        Setembro = 9,
        Outubro = 10,
        Novembro = 11,
        Dezembro = 12,
    }
}
