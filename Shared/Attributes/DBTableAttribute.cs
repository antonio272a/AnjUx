
namespace AnjUx.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DBTableAttribute(string table, string? schema = null) : Attribute
    {
        public string Table { get; set; } = table;
        public string? Schema { get; set; } = schema;
    }
}
