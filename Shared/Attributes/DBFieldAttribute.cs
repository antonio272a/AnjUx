
namespace AnjUx.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DBFieldAttribute(string? fieldName = null) : Attribute
    {
        public string? FieldName { get; set; } = fieldName;
    }
}
