
namespace AnjUx.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DBChildrenAttribute(string? name = null) : Attribute
    {
        public string? Name { get; set; } = name;
    }
}
