using AnjUx.Shared.Models.Data;
using AnjUx.Shared.Models.Enums;

namespace AnjUx.Shared.Models.Requests
{
    public class RequestComparador
    {
        public int? AnoInicial { get; set; }
        public Mes? MesInicial { get; set; }
        public int? AnoFinal { get; set; }
        public Mes? MesFinal { get; set; }
        public List<Municipio>? Municipios { get; set; }
    }
}
