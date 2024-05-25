using AnjUx.Shared.Extensions;

namespace AnjUx.ORM.Classes
{
    public enum OrderTipo : int
    {
        ASC = 0,
        DESC = 1,
    }

    public class OrderBy
    {
        public string? Campo { get; set; }
        public string? CampoCompleto { get; set; }
        public Join? Join { get; set; }
        public OrderTipo OrderTipo { get; set;}

        public OrderBy(string campoCompleto, OrderTipo orderTipo = OrderTipo.ASC) 
        {
            CampoCompleto = campoCompleto;
            OrderTipo = orderTipo;
        }

        public OrderBy(Join join, string campo, OrderTipo orderTipo = OrderTipo.ASC) 
        {
            Join = join;
            Campo = campo;
            OrderTipo = orderTipo;
        }

        public override string ToString()
        {
            if (!CampoCompleto.IsNullOrWhiteSpace()) return $"{CampoCompleto} {OrderTipo.GetDescriptionEnum()}";

            return $"{Join!.Alias}.{Campo} {OrderTipo.GetDescriptionEnum()}";
        }

        public static OrderTipo TipoPorBooleano(bool? decrescente)
            => decrescente.True()
                ? OrderTipo.DESC
                : OrderTipo.ASC;
    }
}
