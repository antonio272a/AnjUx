using AnjUx.Client.Shared;

namespace AnjUx.Client.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HubIconAttribute(Icon icone, string titulo) : Attribute
    {
        public Icon Icone { get; set; } = icone;
        public string Titulo { get; set; } = titulo;
    }
}
